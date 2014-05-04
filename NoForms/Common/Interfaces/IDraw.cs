using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX.Direct2D1;

namespace NoForms
{
    /// <summary>
    /// This is abstracted rendering system, available to all componoents to do drawing operations.
    /// </summary>
    public interface IDraw
    {
        Renderers.IUnifiedDraw uDraw { get; }
        Renderers.IRenderElements backRenderer { get; }
        Renderers.UnifiedEffects uAdvanced { get; }
    }
}