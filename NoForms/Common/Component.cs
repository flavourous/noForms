using System;
using System.Windows.Forms;
using SysRect = System.Drawing.Rectangle;

namespace NoForms.Controls.Templates
{
    public abstract class Component : IComponent
    {
        public IComponent Parent { get; set; }
        ComponentCollection _components;
        public ComponentCollection components
        {
            get { return _components; }
        }

        public Component()
        {
            _components = new ComponentCollection(this);
        }

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
        protected event Action<Size> SizeChanged;
        protected void OnSizeChanged()
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
        protected event Action<Point> LocationChanged;
        protected void OnLocationChanged()
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
            Point ploc = new Point(0, 0);
            if (Parent != null && !(Parent is NoForm))
                ploc = Parent.DisplayRectangle.Location;

            this._DisplayRectangle.Location = new Point(
                    ploc.X + _Location.X,
                    ploc.Y + _Location.Y
                    );

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

        Rectangle clipSet = Rectangle.Empty;
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
            rt.uDraw.PushAxisAlignedClip(clipSet);
        }
        internal bool doClip = true;

        /// <summary>
        /// You MUST call this base method when you override, at the end of your method, to
        /// draw the children.
        /// </summary>
        /// <typeparam name="RenderType"></typeparam>
        /// <param name="renderArgument"></param>
        /// <param name="parentDisplayRectangle"></param>
        public void DrawBase(IRenderType renderArgument)
        {
            Draw(renderArgument);
            if(doClip) renderArgument.uDraw.PushAxisAlignedClip(clipSet = DisplayRectangle);
            foreach (IComponent c in components)
                if (c.visible)
                    c.DrawBase(renderArgument);
            if (doClip) renderArgument.uDraw.PopAxisAlignedClip();
        }
        public abstract void Draw(IRenderType renderArgument);
        public virtual void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
            foreach (IComponent c in components)
            {
                if (c.visible)
                    c.MouseMove(location,
                        Util.CursorInRect(c.DisplayRectangle,
                        Util.GetTopLevelLocation(this)), amClipped ? true : 
                            !Util.CursorInRect(DisplayRectangle, Util.GetTopLevelLocation(this)));
            }
        }
        public virtual void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            foreach (IComponent c in components)
            {
                if (c.visible)
                    c.MouseUpDown(mea, mbs, Util.CursorInRect(c.DisplayRectangle, Util.GetTopLevelLocation(c)), 
                        amClipped ? true : !Util.CursorInRect(DisplayRectangle, Util.GetTopLevelLocation(this)));
            }
        }
        public virtual void KeyDown(System.Windows.Forms.Keys key)
        {
            foreach (IComponent inc in components)
                    inc.KeyDown(key);
        }
        public virtual void KeyUp(System.Windows.Forms.Keys key)
        {
            foreach (IComponent inc in components)
                    inc.KeyUp(key);
        }
        public virtual void KeyPress(char c)
        {
            foreach (IComponent inc in components)
                    inc.KeyPress(c);
        }

        bool _visible = true;
        public bool visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public NoForm TopLevelForm
        {
            get
            {
                IComponent ic = Parent;
                while (ic.Parent != null)
                    ic = ic.Parent;
                return ic as NoForm;
            }
        }
    }
}
