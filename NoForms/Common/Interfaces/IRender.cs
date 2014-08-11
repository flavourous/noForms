using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using Common;

namespace NoForms
{
    // Interfaces
    public interface IRender : IDisposable
    {
        void Init(NoForm nf, CreateOptions co, out IWindow w, out IController c);
        void BeginRender();
        void EndRender(MethodInvoker endedCallback);
        NoForm noForm { get; set; }
    }
}