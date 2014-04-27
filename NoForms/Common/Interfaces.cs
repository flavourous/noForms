using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using SysRect = System.Drawing.Rectangle;

namespace NoForms
{
    // Interfaces
    public interface IRender
    {
        void Init(ref Form winForm);
        void BeginRender();
        void EndRender(MethodInvoker endedCallback);
        NoForm noForm { get; set; }
    }

    // FIXME ISP?
    public interface IComponent
    {
        /// <summary>
        /// So that we can query the parent displayrectangle
        /// </summary>
        IComponent Parent { get; set; }
        ComponentCollection components { get; }

        // Properties
        Point Location { get; set; }
        Size Size { get; set; }
        Rectangle DisplayRectangle { get; set; }
        bool visible { get; set; }
        bool IsDisplayRectangleCalculated { get; }
        int ZIndex { get; }

        // events
        event Action<IComponent> ZIndexChanged;
        event Action<Size> SizeChanged;
        event Action<Point> LocationChanged;

        void RecalculateDisplayRectangle();
        void RecalculateLocation();

        // Rendering Support, passing one object for requesting each rendering type
        void DrawBase(IRenderType renderArgument);

        // Mouse events
        void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped);
        void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped);

        // A container is focusable...
        // Key Events
        void KeyDown(System.Windows.Forms.Keys key);
        void KeyUp(System.Windows.Forms.Keys key);
        void KeyPress(char c);

        // cursor...
        Cursor Cursor { get; set; }
        bool Scrollable { get; set; }

        // Clipping control FIXME belongs here?
        void ReClipAll(IRenderType renderArgument);
        void UnClipAll(IRenderType renderArgument);
    }

    public interface IRenderType
    {
        Renderers.IUnifiedDraw uDraw { get; }
        Renderers.IRenderElements backRenderer { get; }
        Renderers.UnifiedEffects uAdvanced { get; }
    }

}