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
            public WFBase(WindowCreateOptions co)
            {
                ShowInTaskbar = co.showInTaskbar;
                switch (co.windowBorderStyle)
                {
                    default:
                    case WindowBorderStyle.Sizable:
                        FormBorderStyle = FormBorderStyle.Sizable;
                        break;
                    case WindowBorderStyle.Fixed:
                        FormBorderStyle = FormBorderStyle.FixedDialog;
                        break;
                    case WindowBorderStyle.NoBorder:
                        FormBorderStyle = FormBorderStyle.None;
                        break;
                }
                //SetStyle(System.Windows.Forms.ControlStyles.UserMouse, true);
                //SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true);
            }
            protected override void OnPaint(PaintEventArgs e) { }
            protected override void OnPaintBackground(PaintEventArgs e) { }

        }

        protected WFBase winForm;
        protected void ProcessCreateOptions(WindowCreateOptions co)
        {
            winForm = new WFBase(co);
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
        Action<NoForm> hookAction;
        public WinForms(IRender<IWFWin> renderer, IController<IWFWin> controller, WindowCreateOptions co)
        {
            ProcessCreateOptions(co);
            hookAction = nf =>
            {
                renderer.Init(this, nf);
                controller.Init(this, nf);
                winForm.Shown += (o, e) => renderer.BeginRender();
                winForm.FormClosing += (o, e) => renderer.EndRender();
                nf.window = this;
            };
        }
        void IPlatform.Init(NoForm toDisplay)
        {
            // do the form
            hookAction(toDisplay);
        }

        public Form form
        {
            get { return winForm; }
        }
    }
    
    
}