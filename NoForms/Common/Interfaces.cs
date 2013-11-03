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
    public interface IComponent
    {
        /// <summary>
        /// So that we can query the parent displayrectangle
        /// </summary>
        IContainer Parent { get; set; }

        // Properties
        Rectangle DisplayRectangle { get; set; }
        bool visible { get; set; }

        void RecalculateDisplayRectangle();
        void RecalculateLocation();

        // Rendering Support, passing one object for requesting each rendering type
        void DrawBase<RenderType>(RenderType renderArgument) /* where RenderType : IRenderType */;

        // Mouse events
        void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped);
        void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped);
    }

    public interface IRenderType
    {
        // TODO Unified interface for render objects, incase we want to add more to existing ones.
        Renderers.UnifiedDraw uDraw { get; }
        Object backRenderer { get; }
        Renderers.UnifiedEffects uAdvanced { get; }
    }

    public interface IContainer
    {
        Rectangle DisplayRectangle { get; set; }
        ComponentCollection components { get; }
        IContainer Parent { get; set; }

        // Clipping control
        void ReClipAll<RenderType>(RenderType renderArgument);
        void UnClipAll<RenderType>(RenderType renderArgument);

        Point Location { get; set; }

        // A container is focusable...
        // Key Events
        void KeyDown(System.Windows.Forms.Keys key);
        void KeyUp(System.Windows.Forms.Keys key);
    }
}