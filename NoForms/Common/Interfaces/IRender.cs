using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        void Dirty(Rectangle rect);
        /// <summary>
        /// Time spent in last rendering pass, not including any frame rate limiting.
        /// </summary>
        float lastFrameRenderDuration { get; }
        event Action<Size> RenderSizeChanged;
        float FPSLimit { get; set; }
    }
}