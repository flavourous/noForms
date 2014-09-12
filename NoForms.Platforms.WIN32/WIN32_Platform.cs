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
            renderer.Init(this, toDisplay);
            controller.Init(this, toDisplay);
            winForm.Shown += (o, e) => renderer.BeginRender();
            winForm.FormClosing += (o, e) => renderer.EndRender();
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

}