using System;
using NoForms.Renderers;
using NoForms.Common;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace NoForms.Windowing.WinForms
{
    // Base Renderers, exposing some drawing mechanism and options
    public class SDGNormal : IRender, IDraw, IWindow, IController
    {
        #region IWindow - Providing window definition and maintanance etc
        class SDGForm : System.Windows.Forms.Form
        {
            public SDGForm() : base()
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

        public NoForms.Common.Cursors Cursor
        {
            get { return Converters.Translate(winForm.Cursor); }
            set { winForm.Cursor = Converters.Translate(value); }
        }

        public bool CaptureMouse
        {
            get { return winForm.Capture; }
            set { winForm.Capture = value; }
        }

        public void SetClipboard(String s)
        {
            System.Windows.Forms.Clipboard.SetText(s);
        }
        public void GetClipboard(out String s)
        {
            s = System.Windows.Forms.Clipboard.GetText();
        }

        #endregion

        #region IRender - Bulk of class, providing rendering control (specialised to particular IWindow)
        Form winForm;
        Graphics graphics;
        Bitmap buffer;

        public SDGNormal()
        {
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
                winForm = new SDGForm();
                ProcessCreateOptions(co);

                // Create buffer
                buffer = new Bitmap(winForm.Width, winForm.Height);
                graphics = Graphics.FromImage(buffer);

                // Init uDraw and assign IRenderElement parts
                _backRenderer = new SDG_RenderElements(graphics);
                _uDraw = new SDGDraw(_backRenderer);

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

        

        public Thread renderThread = null;
        public void BeginRender()
        {
            renderThread = new Thread(new ThreadStart(() =>
            {
                while (running)
                    RenderPass();
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
                noForm.DrawBase(this);

                // flush buffer to window
                var winGr = winForm.CreateGraphics();
                winGr.DrawImageUnscaled(buffer, new System.Drawing.Point(0, 0));
                winGr.Dispose();
            }
        }
        void Resize()
        {
            winForm.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                graphics.Dispose();
                buffer.Dispose();
                winForm.ClientSize = new System.Drawing.Size((int)noForm.Size.width , (int)noForm.Size.height );
                winForm.Location = SDGTr.trI(noForm.Location);
                buffer = new Bitmap(winForm.Width, winForm.Height);
                graphics = Graphics.FromImage(buffer);
                _backRenderer.graphics = graphics;
            }));
        }

        #endregion

        #region IDraw - Client facing interface to drawing commands and so on
        IUnifiedDraw _uDraw;
        public IUnifiedDraw uDraw
        {
            get { return _uDraw; }
        }
        SDG_RenderElements _backRenderer;
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
            winForm.MouseDown += (o,e) => MouseUpDown(SDGTr.tr(e.Location), ConvertFromWinForms(e.Button), NoForms.Common.ButtonState.DOWN);
            winForm.MouseUp += (o, e) => MouseUpDown(SDGTr.tr(e.Location), ConvertFromWinForms(e.Button), NoForms.Common.ButtonState.UP);
            winForm.MouseMove += (o,e) => MouseMove(SDGTr.tr(e.Location));
            winForm.KeyDown += (o, e) => KeyUpDown((NoForms.Common.Keys)e.KeyCode, NoForms.Common.ButtonState.DOWN);
            winForm.KeyUp += (o, e) => KeyUpDown((NoForms.Common.Keys)e.KeyCode, NoForms.Common.ButtonState.UP);
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
        public NoForms.Common.Point MouseScreenLocation
        {
            get { return SDGTr.tr(System.Windows.Forms.Cursor.Position); }
        }
        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }    
}
