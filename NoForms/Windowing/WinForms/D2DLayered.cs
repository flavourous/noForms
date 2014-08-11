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
    // Base Renderers, exposing some drawing mechanism and options
    public class D2DLayered : IRender, IDraw, IWindow, IController
    {
        #region IWindow - Providing window definition and maintanance etc
        class D2LForm : System.Windows.Forms.Form
        {
            public D2LForm() : base()
            {
                SetStyle(System.Windows.Forms.ControlStyles.UserMouse, true);
                SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
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

        public void Run()
        {
            Application.EnableVisualStyles();
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
            return true;
        }
        public bool Restore()
        {
            winForm.WindowState = FormWindowState.Normal;
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

        #endregion

        #region IRender - Bulk of class, providing rendering control (specialised to particular IWindow)
        SharpDX.Direct3D10.Device1 device;
        SharpDX.Direct2D1.Factory d2dFactory = new SharpDX.Direct2D1.Factory();
        SharpDX.DXGI.Factory dxgiFactory = new SharpDX.DXGI.Factory();

        Texture2D backBuffer;
        RenderTargetView renderView;
        Surface1 surface;
        RenderTarget d2dRenderTarget;
        IntPtr winHandle;
        Form winForm;
        IntPtr someDC;

        public D2DLayered()
        {
            device = new SharpDX.Direct3D10.Device1(DriverType.Hardware, DeviceCreationFlags.BgraSupport, SharpDX.Direct3D10.FeatureLevel.Level_10_1);
            someDC = Win32.GetDC(IntPtr.Zero); // not sure what this exactly does... root dc perhaps...
        }
        void ProcessCreateOptions(CreateOptions co)
        {
            winForm.ShowInTaskbar = co.showInTaskbar;
        }
        public void Init(NoForm root, CreateOptions co, out IWindow window, out IController controller)
        {
            noForm = root;
            lock (noForm)
            {
                // do the form
                winForm = new D2LForm();
                ProcessCreateOptions(co);
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
                _uDraw = new D2DDraw(_backRenderer);

                // FIXME these should also be abstracted, preferably before I have more than a couple of renderers.
                window = this;
                controller = this;
            }

            winForm.Load += new EventHandler((object o, EventArgs e) =>
            {
                BeginRender();
            });

            winForm.FormClosing += new FormClosingEventHandler((object o, FormClosingEventArgs e) =>
            {
                if (!okClose) e.Cancel = true;
                EndRender(new MethodInvoker(() => Close(true)));
            });

            ControllerRegistration();
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
            // Resize the form and backbuffer to noForm.Size, and fire the noForms sizechanged
            Resize();

            lock (noForm)
            {
                // Do Drawing stuff
                DrawingSize rtSize = new DrawingSize((int)d2dRenderTarget.Size.Width, (int)d2dRenderTarget.Size.Height);
                d2dRenderTarget.BeginDraw();
                d2dRenderTarget.PushAxisAlignedClip(noForm.DisplayRectangle, AntialiasMode.Aliased);
                noForm.DrawBase(this);
                d2dRenderTarget.PopAxisAlignedClip();

                // Fill with transparency the edgeBuffer!
                d2dRenderTarget.FillRectangle(new RectangleF(0, noForm.Size.height, noForm.Size.width + edgeBufferSize, noForm.Size.height + edgeBufferSize), scbTrans);
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
        }
        public int edgeBufferSize = 128;
        void Resize()
        {
            d2dRenderTarget.Dispose();
            renderView.Dispose();
            surface.Dispose();
            backBuffer.Dispose();

            winForm.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                winForm.ClientSize = new System.Drawing.Size((int)noForm.Size.width + edgeBufferSize, (int)noForm.Size.height + edgeBufferSize);
                winForm.Location = noForm.Location;
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

        public void Dispose()
        {
            d2dFactory.Dispose();
            dxgiFactory.Dispose();
            device.Dispose();
        }
        #endregion

        #region IDraw - Client facing interface to drawing commands and so on
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
        #endregion

        #region IController - Provides input.  May be intimately linked to IWindow (eg WinForms) or not (eg DirectInput)
        void ControllerRegistration()
        {
            winForm.MouseDown += (o, e) => MouseUpDown(e.Location, ConvertFromWinForms(e.Button), Common.ButtonState.DOWN);
            winForm.MouseUp += (o, e) => MouseUpDown(e.Location, ConvertFromWinForms(e.Button), Common.ButtonState.UP);
            winForm.MouseMove += (o,e) => MouseMove(e.Location);
            winForm.KeyDown += (o, e) => KeyUpDown(e.KeyCode, Common.ButtonState.DOWN);
            winForm.KeyUp += (o,e) => KeyUpDown(e.KeyCode, Common.ButtonState.UP);
            winForm.KeyPress += (o,e) => KeyPress(e.KeyChar);
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

      }    
}
