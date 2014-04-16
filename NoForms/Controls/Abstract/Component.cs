using System;
using System.Windows.Forms;
using SysRect = System.Drawing.Rectangle;

namespace NoForms.Controls.Abstract
{
    public abstract class Component : IComponent
    {
        public IComponent Parent { get; set; }
        ComponentCollection _components = new AlwaysEmptyComponentCollection(null);
        public virtual ComponentCollection components
        {
            get
            {
                return _components;
            }
        }

        bool idrs = true;
        public bool IsDisplayRectangleCalculated { get { return idrs; } set { idrs = value; } }
        protected Rectangle _DisplayRectangle = Rectangle.Empty;
        /// <summary>
        /// Display rectangle is relative to the RenderType, i.e. the top level component
        /// In contrast to Size and Location (which are modified when this is changed)
        /// </summary>
        public Rectangle DisplayRectangle
        {
            get { return _DisplayRectangle; }
            set
            {
                _DisplayRectangle = value;
                RecalculateLocation();
                Size = value.Size;
            }
        }
        public event Action<Size> SizeChanged;
        protected virtual void OnSizeChanged()
        {
            if (SizeChanged != null)
                foreach (Action<Size> se in SizeChanged.GetInvocationList())
                    se(_DisplayRectangle.Size);
        }
        /// <summary>
        /// Size Of Control. Modifies DisplayRectangle Size.
        /// </summary>
        public Size Size
        {
            get { return _DisplayRectangle.Size; }
            set
            {
                _DisplayRectangle.Size = value;
                OnSizeChanged();
            }
        }
        public event Action<Point> LocationChanged;
        protected virtual void OnLocationChanged()
        {
            if (LocationChanged != null)
                foreach (Action<Point> pe in LocationChanged.GetInvocationList())
                    pe(_Location);
        }
        protected Point _Location = Point.Empty;
        /// <summary>
        /// Location of control relative to parent control.
        /// This also modifies DisplayRectangle.
        /// </summary>
        public Point Location
        {
            get
            {
                return _Location;
            }
            set
            {
                _Location = value;
                OnLocationChanged();
                RecalculateDisplayRectangle();
            }
        }

        public void RecalculateDisplayRectangle()
        {
            if (IsDisplayRectangleCalculated)
            {
                Point ploc = new Point(0, 0);
                if (Parent != null && !(Parent is NoForm))
                    ploc = Parent.DisplayRectangle.Location;

                this._DisplayRectangle.Location = new Point(
                        ploc.X + _Location.X,
                        ploc.Y + _Location.Y
                        );
            }
            foreach (IComponent c in components)
                c.RecalculateDisplayRectangle();
        }
        public void RecalculateLocation()
        {
            Point ploc = new Point(0, 0);
            if (Parent != null && !(Parent is NoForm))
                ploc = Parent.DisplayRectangle.Location;

            // Location is the difference between displayrectangle positions
            _Location = new Point((int)DisplayRectangle.left - ploc.X, (int)DisplayRectangle.top - ploc.Y);

            foreach (IComponent c in components)
                c.RecalculateLocation();
        }

        protected Rectangle clipSet = Rectangle.Empty;
        public void UnClipAll(IRenderType rt)
        {
            if (!doClip) return;
            rt.uDraw.PopAxisAlignedClip();
            Parent.UnClipAll(rt);
        }
        public void ReClipAll(IRenderType rt)
        {
            if (!doClip) return;
            Parent.ReClipAll(rt);
            rt.uDraw.PushAxisAlignedClip(clipSet,false);
        }
        internal bool doClip = true;

        public abstract void DrawBase(IRenderType renderArgument);

        bool _visible = true;
        public bool visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public Cursor Cursor { get; set; }
        public bool _Scrollable = true;
        public bool Scrollable { get { return _Scrollable; } set { _Scrollable = value; } }

        public virtual void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped) { }
        public virtual void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped) { }
        public virtual void KeyDown(Keys key) { }
        public virtual void KeyUp(Keys key) { }
        public virtual void KeyPress(char c) { }
    }
}
