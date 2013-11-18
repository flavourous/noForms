using System;
using System.Threading;
using System.Diagnostics;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX;

namespace NoForms.Renderers
{
    public class D2DSwapChain : IRender, IRenderType
    {
        class D2SForm : System.Windows.Forms.Form
        {
            public D2SForm()
                : base()
            {
                SetStyle(System.Windows.Forms.ControlStyles.UserMouse, true);
                SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            }

            //protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
            //{
            //    //return base.ProcessCmdKey(ref msg, keyData);
            //    return false;
            //}
            //protected override bool ProcessDialogKey(System.Windows.Forms.Keys keyData)
            //{
            //    //return base.ProcessDialogKey(keyData);
            //    return false;
            //}
            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                SetRegion(Size);
            }
            public void SetRegion(System.Drawing.Size RealClientSize)
            {
                if (FormBorderStyle != System.Windows.Forms.FormBorderStyle.None) return;
                // default region can be funny
                System.Drawing.Rectangle nr = new System.Drawing.Rectangle(ClientRectangle.Location, RealClientSize);
                var myReg = new System.Drawing.Region(nr);
                Region = myReg;
            }

            protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
            {
                //base.OnPaint(e);
            }
            protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
            {
                //base.OnPaintBackground(e);
            }
        }

        SharpDX.Direct3D10.Device1 device;
        SharpDX.Direct2D1.Factory d2dFactory = new SharpDX.Direct2D1.Factory();
        SharpDX.DXGI.Factory dxgiFactory = new SharpDX.DXGI.Factory();

        SwapChain swapchain;
        Texture2D backBuffer;
        RenderTargetView renderView;
        Surface1 surface;
        RenderTarget d2dRenderTarget;

        IntPtr winHandle;

        public D2DSwapChain()
        {

        }
        public void Init(ref System.Windows.Forms.Form winForm)
        {
            // do the form
            winForm = new D2SForm();
            System.Windows.Forms.Form wfCopy = winForm;
            winHandle = winForm.Handle;

            winForm.LocationChanged += new EventHandler((object o, EventArgs e) =>
            {
                noForm.Location = wfCopy.Location;
            });
            winForm.SizeChanged += new EventHandler((object o, EventArgs e) =>
            {
                noForm.Size = wfCopy.ClientSize;
            });

            SwapChainDescription swapchainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                Flags = SwapChainFlags.GdiCompatible,
                IsWindowed = true,
                ModeDescription = new ModeDescription((int)noForm.Size.width, (int)noForm.Size.height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                OutputHandle = winHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Sequential,
                Usage = Usage.RenderTargetOutput
            };
            SharpDX.Direct3D10.Device1.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, swapchainDescription, out device, out swapchain);

            // Initialise d2d things
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapchain, 0);
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

            // Init uDraw and assign IRenderElement parts
            _backRenderer = new D2D_RenderElements(d2dRenderTarget);
            _uDraw = new UnifiedDraw(_backRenderer);

        }
        public Thread renderThread = null;
        public void BeginRender()
        {
            renderThread = new Thread(new ThreadStart(() =>
            {
                while (running)
                    RenderPass();
                ended();
            }));
            renderThread.Start();
        }
        System.Windows.Forms.MethodInvoker ended;
        bool running = true;
        public void EndRender(System.Windows.Forms.MethodInvoker ender)
        {
            ended = ender;
            running = false;
        }

        // object because IRender could be anything, gdi, opengl etc...
        public NoForm noForm { get; set; }
        void RenderPass()
        {
            // Resize the form and backbuffer to noForm.Size
            Resize();

            // Do Drawing stuff
            DrawingSize rtSize = new DrawingSize((int)d2dRenderTarget.Size.Width, (int)d2dRenderTarget.Size.Height);
            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.PushAxisAlignedClip(noForm.DisplayRectangle, AntialiasMode.Aliased);
            noForm.DrawBase(this);
            d2dRenderTarget.PopAxisAlignedClip();
            d2dRenderTarget.EndDraw();

            swapchain.Present(0, PresentFlags.None);
        }
        void Resize()
        {
            d2dRenderTarget.Dispose();
            renderView.Dispose();
            surface.Dispose();
            backBuffer.Dispose();

            noForm.theForm.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                noForm.theForm.ClientSize = noForm.Size;
                noForm.theForm.Location = noForm.Location;
                (noForm.theForm as D2SForm).SetRegion(noForm.theForm.Size);
            }));

            swapchain.ResizeBuffers(0, (int)noForm.Size.width, (int)noForm.Size.height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapchain, 0);
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            // Init uDraw and assign IRenderElement parts
            _backRenderer.renderTarget = d2dRenderTarget;
        }

        // IRenderType
        UnifiedDraw _uDraw;
        public UnifiedDraw uDraw
        {
            get { return _uDraw; }
        }
        D2D_RenderElements _backRenderer;
        public IRenderElements backRenderer
        {
            get { return _backRenderer; }
        }
        public UnifiedEffects uAdvanced
        {
            get { throw new NotImplementedException(); }
        }
    }
}
