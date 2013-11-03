using System;
using System.Windows.Forms;

namespace NoForms.Controls.Templates
{
    public abstract class Containable : Focusable
    {
        public override IContainer Parent { get; set; }
        protected Rectangle _DisplayRectangle = Rectangle.Empty;
        /// <summary>
        /// Display rectangle is relative to the RenderType, i.e. the top level component
        /// In contrast to Size and Location (which are modified when this is changed)
        /// </summary>
        public override Rectangle DisplayRectangle
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
        /// <summary>
        /// Gets parent displayrectangle origin, adds this location to it, setting
        /// the location of our displayrectanle.
        /// </summary>
        public override void RecalculateDisplayRectangle()
        {
            Point ploc = new Point(0, 0);
            if (Parent != null && !(Parent is NoForm))
                ploc = Parent.DisplayRectangle.Location;

            this._DisplayRectangle.Location = new Point(
                    ploc.X + _Location.X,
                    ploc.Y + _Location.Y
                    );
        }

        public override void RecalculateLocation()
        {
            Point ploc = new Point(0, 0);
            if (Parent != null && !(Parent is NoForm))
                ploc = Parent.DisplayRectangle.Location;

            // Location is the difference between displayrectangle positions
            _Location = new Point((int)DisplayRectangle.left - ploc.X, (int)DisplayRectangle.top - ploc.Y);
        }

        bool _visible = true;
        public override bool visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

    }
}
