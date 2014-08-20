using System;
using NoForms.Common;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NoForms.Renderers;

namespace NoForms.Windowing
{
    public interface IW32Win { IntPtr handle { get; } }
    public interface IWFWin { Form form { get; } }

    public abstract class WinBase : IWindow
    {
        protected class WFBase : Form, IWin32Window
        {
            public WFBase()
            {
                SetStyle(System.Windows.Forms.ControlStyles.UserMouse, true);
                SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
            }
            protected override void OnPaint(PaintEventArgs e) { }
            protected override void OnPaintBackground(PaintEventArgs e) { }
        }

        protected WFBase winForm;
        protected void ProcessCreateOptions(CreateOptions co)
        {
            winForm.ShowInTaskbar = co.showInTaskbar;
            winForm.FormBorderStyle = co.borderedWindow ? FormBorderStyle.Sizable : FormBorderStyle.None;
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

        public void Close()
        {
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
            get { return WFTr.Translate(winForm.Cursor); }
            set { winForm.Cursor = WFTr.Translate(value); }
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
    }

    public class WinformsController : IController<IWFWin>
    {
        Form winForm;
        NoForm nf;
        void IController<IWFWin>.Init(IWFWin initObj, NoForm nf)
        {
            winForm = initObj.form;
            this.nf = nf;
            nf.controller = this;

            // Register!
            winForm.MouseDown += (o, e) => nf.MouseUpDown(SDGTr.tr(e.Location), WFTr.Translate(e.Button), NoForms.Common.ButtonState.DOWN, true, false);
            winForm.MouseUp += (o, e) => nf.MouseUpDown(SDGTr.tr(e.Location), WFTr.Translate(e.Button), NoForms.Common.ButtonState.UP, true, false);
            winForm.MouseMove += (o, e) => nf.MouseMove(SDGTr.tr(e.Location), true, false);
            winForm.KeyDown += (o, e) => nf.KeyUpDown((NoForms.Common.Keys)e.KeyCode, NoForms.Common.ButtonState.DOWN);
            winForm.KeyUp += (o, e) => nf.KeyUpDown((NoForms.Common.Keys)e.KeyCode, NoForms.Common.ButtonState.UP);
            winForm.KeyPress += (o, e) => nf.KeyPress(e.KeyChar);
        }

        public Point MouseScreenLocation
        {
            get { return SDGTr.tr(System.Windows.Forms.Cursor.Position); }
        }

        public event MouseUpDownHandler MouseUpDown;

        public event MouseMoveHandler MouseMove;

        public event KeyUpDownHandler KeyUpDown;

        public event KeyPressHandler KeyPress;
    }

    public class Win32 : WinBase, IPlatform, IW32Win, IWFWin
    {
        IRender<IW32Win> renderer;
        IController<IWFWin> controller;
        public Win32(IRender<IW32Win> renderer, IController<IWFWin> controller)
        {
            this.renderer = renderer;
            this.controller = controller;
        }

        void IPlatform.Init(NoForm toDisplay, CreateOptions co)
        {
            // do the form
            winForm = new WFBase();
            renderer.Init(this, toDisplay);
            controller.Init(this, toDisplay);
            winForm.Shown += (o, e) => renderer.BeginRender();
            winForm.FormClosing += (o, e) =>
            {
                e.Cancel = renderer.running;
                renderer.stopped += () => winForm.Invoke(new VoidAction(() => winForm.Close()));
                renderer.EndRender();
            };
            ProcessCreateOptions(co);
            toDisplay.window = this;
        }

        public IntPtr handle
        {
            get
            {
                IntPtr han = IntPtr.Zero;
                winForm.Invoke(new VoidAction(() => han = winForm.Handle));
                return han;
            }
        }

        public Form form
        {
            get { return winForm; }
        }
    }

    public class WinForms : WinBase, IPlatform, IWFWin
    {

        IRender<IWFWin> renderer;
        IController<IWFWin> controller;
        public WinForms(IRender<IWFWin> renderer, IController<IWFWin> controller)
        {
            this.renderer = renderer;
            this.controller = controller;
        }
        void IPlatform.Init(NoForm toDisplay, CreateOptions co)
        {
            // do the form
            winForm = new WFBase();
            renderer.Init(this, toDisplay);
            controller.Init(this, toDisplay);

            winForm.Shown += (o, e) => renderer.BeginRender();
            winForm.FormClosing += (o, e) =>
            {
                e.Cancel = renderer.running;
                renderer.stopped += () => winForm.Invoke(new VoidAction(() => winForm.Close()));
                renderer.EndRender();
            };
            ProcessCreateOptions(co);
            toDisplay.window = this;
        }

        public Form form
        {
            get { return winForm; }
        }
    }
    
    
}