using System;
using NoForms.Common;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NoForms.Platforms.DotNet;

namespace NoForms.Platforms.Win32
{
    public interface IW32Win { IntPtr handle { get; } }

    public class Win32 : WinBase, IPlatform, IW32Win, IWFWin
    {
        Action<NoForm> hookAction;
        public Win32(IRender<IW32Win> renderer, IController<IWFWin> controller, WindowCreateOptions co)
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

}