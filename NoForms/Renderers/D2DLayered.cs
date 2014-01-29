using System;
using System.Threading;
using System.Diagnostics;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX;

namespace NoForms.Renderers
{
    // Base Renderers, exposing some drawing mechanism and options
    public class D2DLayered : IRender, IRenderType
    {
        class D2LForm : System.Windows.Forms.Form
        {
            public D2LForm() : base()
            {
                SetStyle(System.Windows.Forms.ControlStyles.UserMouse, true);
                SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            }
      
            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                SetRegion(Size);
            }
            public void SetRegion(System.Drawing.Size RealClientSize) 
            {
                // default region can be funny
                System.Drawing.Rectangle nr = new System.Drawing.Rectangle(ClientRectangle.Location, RealClientSize);
                var myReg = new System.Drawing.Region(nr);
                Region = myReg;
            }
            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams cp = base.CreateParams;
                    cp.ExStyle |= Win32.WS_EX_LAYERED;
                    return cp;
                }
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

        Texture2D backBuffer;
        RenderTargetView renderView;
        Surface1 surface;
        RenderTarget d2dRenderTarget;

        IntPtr winHandle;

        IntPtr someDC;


        public D2DLayered()
        {
            device = new SharpDX.Direct3D10.Device1(DriverType.Hardware, DeviceCreationFlags.BgraSupport, SharpDX.Direct3D10.FeatureLevel.Level_10_1);
            someDC = Win32.GetDC(IntPtr.Zero); // not sure what this exactly does... root dc perhaps...
        }
        public void Init(ref System.Windows.Forms.Form winForm)
        {
            // do the form
            winForm = new D2LForm();
            winHandle = winForm.Handle;

            // Initialise d2d things
            backBuffer = new Texture2D(device, new Texture2DDescription()
            {
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                OptionFlags = ResourceOptionFlags.GdiCompatible,
                Width = winForm.Size.Width,
                Height = winForm.Size.Height,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget,
                Format = Format.B8G8R8A8_UNorm
            });
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            scbTrans = new SolidColorBrush(d2dRenderTarget, new Color4(1f, 0f, 1f, 0f)); // set buffer area to transparent

            // Init uDraw and assign IRenderElement parts
            _backRenderer = new D2D_RenderElements(d2dRenderTarget);
            _uDraw = new UnifiedDraw(_backRenderer);
        }
        SolidColorBrush scbTrans;
        public Thread renderThread = null;
        public void BeginRender()
        {
            renderThread = new Thread(new ThreadStart(() =>
            {
                while (running)
                    RenderPass();

                // Free unmanaged stuff
                scbTrans.Dispose();
                d2dRenderTarget.Dispose();
                surface.Dispose();
                renderView.Dispose();
                backBuffer.Dispose();
                d2dFactory.Dispose();
                dxgiFactory.Dispose();
                device.Dispose();
                ended();
            }));
            running = true;
            renderThread.Start();
        }
        bool running = false;
        System.Windows.Forms.MethodInvoker ended;
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

            // Fill with transparency the edgeBuffer!
            d2dRenderTarget.FillRectangle(new RectangleF(0,noForm.Size.height,noForm.Size.width + edgeBufferSize, noForm.Size.height + edgeBufferSize),scbTrans);
            d2dRenderTarget.FillRectangle(new RectangleF(noForm.Size.width, 0, noForm.Size.width + edgeBufferSize, noForm.Size.height + edgeBufferSize), scbTrans);
            d2dRenderTarget.EndDraw();

            // Present DC to windows (ugh layered windows sad times)
            IntPtr dxHdc = surface.GetDC(false);
            System.Drawing.Graphics dxdc = System.Drawing.Graphics.FromHdc(dxHdc);
            Win32.Point dstPoint = new Win32.Point((int)noForm.Location.X, (int)noForm.Location.Y);
            Win32.Point srcPoint = new Win32.Point(0, 0);
            Win32.Size pSize = new Win32.Size(rtSize.Width, rtSize.Height);
            Win32.BLENDFUNCTION bf = new Win32.BLENDFUNCTION() { SourceConstantAlpha = 255, AlphaFormat = Win32.AC_SRC_ALPHA, BlendFlags = 0, BlendOp = 0 };
            bool suc = Win32.UpdateLayeredWindow(winHandle, someDC, ref dstPoint, ref pSize, dxHdc, ref srcPoint, 1, ref bf, 2);
            surface.ReleaseDC();
            dxdc.Dispose();
        }
        public int edgeBufferSize = 128;
        void Resize()
        {
            d2dRenderTarget.Dispose();
            renderView.Dispose();
            surface.Dispose();
            backBuffer.Dispose();

            noForm.theForm.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                noForm.theForm.ClientSize = new System.Drawing.Size((int)noForm.Size.width + edgeBufferSize, (int)noForm.Size.height + edgeBufferSize);
                (noForm.theForm as D2LForm).SetRegion(noForm.theForm.Size);
            }));

            // Initialise d2d things
            backBuffer = new Texture2D(device, new Texture2DDescription()
            {
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                OptionFlags = ResourceOptionFlags.GdiCompatible,
                Width = (int)noForm.Size.width + edgeBufferSize,
                Height = (int)noForm.Size.height + edgeBufferSize,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget,
                Format = Format.B8G8R8A8_UNorm
            });
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
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
