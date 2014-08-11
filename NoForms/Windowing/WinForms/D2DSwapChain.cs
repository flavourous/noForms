using System;
using System.Threading;
using System.Diagnostics;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX;
using System.Windows.Forms;
using Common;

namespace NoForms.Renderers
{
    public class D2DSwapChain : IRender, IDraw, IWindow, IController
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

            protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
            {
                //base.OnPaint(e);
            }
            protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
            {
                //base.OnPaintBackground(e);
            }
        }

        public void Run()
        {
            //Application.EnableVisualStyles();
            Application.Run(winForm);
        }
        public void Show()
        {
            winForm.Show();
        }
        public void ShowDialog()
        {
            winForm.ShowDialog();
        }
        public void Hide()
        {
            winForm.Hide();
        }

        bool okClose = false;
        public void Close()
        {
            Close(false);
        }
        void Close(bool done)
        {
            if (winForm.InvokeRequired)
            {
                winForm.Invoke(new MethodInvoker(() => Close(done)));
                return;
            }
            okClose = done;
            winForm.Close();
        }

        public bool Minimise()
        {
            winForm.WindowState = FormWindowState.Minimized;
            return true;
        }
        public bool Maximise()
        {
            winForm.WindowState = FormWindowState.Maximized;
            oldSize = noForm.Size;
            noForm.Size = winForm.Size;
            return true;
        }
        Size oldSize;
        public bool Restore()
        {
            winForm.WindowState = FormWindowState.Normal;
            noForm.Size = oldSize;
            return true;
        }

        public string Title
        {
            get { return winForm.Text; }
            set { winForm.Text = value; }
        }

        public bool showIcon
        {
            get { return winForm.ShowIcon; }
            set { winForm.ShowIcon = value; }
        }

        public System.Drawing.Icon Icon
        {
            get { return winForm.Icon; }
            set { winForm.Icon = value; }
        }

        public bool BringToFront()
        {
            winForm.BringToFront();
            return true;
        }

        public Cursor Cursor
        {
            get { return winForm.Cursor; }
            set { winForm.Cursor = value; }
        }

        public bool CaptureMouse
        {
            get { return winForm.Capture; }
            set { winForm.Capture = value; }
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
        Form winForm;
        public D2DSwapChain()
        {

        }
        void ProcessCreateOptions(CreateOptions co)
        {
            winForm.ShowInTaskbar = co.showInTaskbar;
        }
        public void Init(NoForm root, CreateOptions co, out IWindow win, out IController con)
        {
            // do the form
            noForm = root;
            winForm = new D2SForm();
            ProcessCreateOptions(co);
            winHandle = winForm.Handle;

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
            _uDraw = new D2DDraw(_backRenderer);

            ControllerRegistration();

            winForm.Load += new EventHandler((object o, EventArgs e) =>
            {
                BeginRender();
            });

            winForm.FormClosing += new FormClosingEventHandler((object o, FormClosingEventArgs e) =>
            {
                if (!okClose) e.Cancel = true;
                EndRender(new MethodInvoker(() => Close(true)));
            });

            win = this;
            con = this;
        }
        public Thread renderThread = null;
        public void BeginRender()
        {
            renderThread = new Thread(new ThreadStart(() =>
            {
                while (running)
                    RenderPass();

                // free unmanaged
                d2dRenderTarget.Dispose();
                surface.Dispose();
                renderView.Dispose();
                backBuffer.Dispose();
                swapchain.Dispose();
                device.Dispose();

                ended();
            }));
            running = true;
            renderThread.Start();
        }
        System.Windows.Forms.MethodInvoker ended;
        bool running = true;
        public void EndRender(System.Windows.Forms.MethodInvoker ender)
        {
            ended = ender;
            running = false;
        }

        public NoForm noForm { get; set; }
        void RenderPass()
        {
            lock (noForm)
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

                winForm.BeginInvoke(new System.Windows.Forms.MethodInvoker(() =>
                {
                    winForm.Size = noForm.Size;
                    winForm.Location = noForm.Location;
                }));

                swapchain.Present(0, PresentFlags.None);
            }
        }
        void Resize()
        {
            d2dRenderTarget.Dispose();
            renderView.Dispose();
            surface.Dispose();
            backBuffer.Dispose();

            

            swapchain.ResizeBuffers(0, (int)noForm.Size.width, (int)noForm.Size.height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapchain, 0);
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            _backRenderer.renderTarget = d2dRenderTarget;
        }

        // IRenderType
        IUnifiedDraw _uDraw;
        public IUnifiedDraw uDraw
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

        #region IController - Provides input.  May be intimately linked to IWindow (eg WinForms) or not (eg DirectInput)
        void ControllerRegistration()
        {
            winForm.MouseDown += (o, e) => MouseUpDown(e.Location, ConvertFromWinForms(e.Button), Common.ButtonState.DOWN);
            winForm.MouseUp += (o, e) => MouseUpDown(e.Location, ConvertFromWinForms(e.Button), Common.ButtonState.UP);
            winForm.MouseMove += (o, e) => MouseMove(e.Location);
            winForm.KeyDown += (o, e) => KeyUpDown(e.KeyCode, Common.ButtonState.DOWN);
            winForm.KeyUp += (o, e) => KeyUpDown(e.KeyCode, Common.ButtonState.UP);
            winForm.KeyPress += (o, e) => KeyPress(e.KeyChar);
        }
        MouseButton ConvertFromWinForms(MouseButtons mb)
        {
            switch (mb)
            {
                case MouseButtons.Left:
                    return MouseButton.LEFT;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    return MouseButton.RIGHT;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    return MouseButton.NONE;
            }
            return MouseButton.NONE;
        }
        public event MouseUpDownHandler MouseUpDown = delegate { };
        public event MouseMoveHandler MouseMove = delegate { };
        public event KeyUpDownHandler KeyUpDown = delegate { };
        public event KeyPressHandler KeyPress = delegate { };
        public Point MouseScreenLocation
        {
            get { return System.Windows.Forms.Cursor.Position; }
        }
        #endregion


        public void Dispose()
        {
            d2dFactory.Dispose();
            dxgiFactory.Dispose();
        }
    }
}
