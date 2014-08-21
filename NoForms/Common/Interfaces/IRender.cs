using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using NoForms.Common;

namespace NoForms
{
    // Interfaces
    public interface IRender<T> : IRender
    {
        void Init(T initObj, NoForm nf);
    }
    public interface IRender : IDisposable
    {
        void BeginRender();
        void EndRender();
        NoForm noForm { get; set; }
    }
}