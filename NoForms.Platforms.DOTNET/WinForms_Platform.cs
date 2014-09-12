using System;
using NoForms.Common;
using System.Windows.Forms;

namespace NoForms.Platforms.DotNet
{
    public interface IWFWin { Form form { get; } }

    public abstract class WinBase : IWindow
    {
        protected class WFBase : Form
        {
            public WFBase()
            {
                SetStyle(System.Windows.Forms.ControlStyles.UserMouse, true);
                SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
            }
            protected override void OnPaint(PaintEventArgs e) { }
            protected override void OnPaintBackground(PaintEventArgs e) { }

        }

        protected WFBase winForm = new WFBase();
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

        UBitmap[] icon;
        public UBitmap[] Icon
        {
            get { return icon; }
            set { winForm.Icon = WFTr.Translate(icon=value); }
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
            renderer.Init(this, toDisplay);
            controller.Init(this, toDisplay);

            winForm.Shown += (o, e) => renderer.BeginRender();
            winForm.FormClosing += (o, e) => renderer.EndRender();
            ProcessCreateOptions(co);
            toDisplay.window = this;
        }

        public Form form
        {
            get { return winForm; }
        }
    }
    
    
}