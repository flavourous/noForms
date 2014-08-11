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
        int ZIndex { get; }

        // events
        event Action<IComponent> ZIndexChanged;
        event Action<Size> SizeChanged;
        event Action<Point> LocationChanged;

        void RecalculateDisplayRectangle();
        void RecalculateLocation();

        // Rendering Support, passing one object for requesting each rendering type
        void DrawBase(IDraw renderArgument);

        // Mouse events
        void MouseMove(Point location, bool inComponent, bool amClipped);
        void MouseUpDown(Point location, MouseButton mb, Common.ButtonState mbs, bool inComponent, bool amClipped);

        // A container is focusable...
        // Key Events
        void KeyUpDown(System.Windows.Forms.Keys key, Common.ButtonState bs);
        void KeyPress(char c);

        // cursor...
        Cursor Cursor { get; set; }
        bool Scrollable { get; set; }
    }

}