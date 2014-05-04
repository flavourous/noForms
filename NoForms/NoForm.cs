using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;
using NoForms.Controls.Abstract;
using NoForms.Renderers;

namespace NoForms
{
    public class CreateOptions
    {
        public bool showInTaskbar = true;
        public bool rootForm = true;
        public bool dialog = false;
    }
    /// <summary>
    /// You would create a noform, and to it add INoComponent, and it would handle stuff.
    /// </summary>
    public class NoForm : IComponent
    {
        public IComponent Parent { get { return null; } set { } }
        ComponentCollection _components;
        public ComponentCollection components
        {
            get { return _components; }
        }
        public int ZIndex { get { return 0; } }
        public event Action<IComponent> ZIndexChanged = delegate { };

        protected IRender renderMethod;

        public Cursor Cursor { get; set; } // FIXME does nothing.
        public bool Scrollable { get; set; } // Begins to be irellivant FIXME?

        // Some Model Elements...
        Point _Location = new Point(0, 0);
        public Point Location
        {
            get { return _Location; }
            set
            {
                _Location = value;
                LocationChanged(_Location);
            }
        }
        Size _Size = new Size(200, 300);
        /// <summary>
        /// This mirrors the size propterty at 0,0
        /// </summary>
        private Rectangle _DisplayRectangle = new Rectangle();
        public Rectangle DisplayRectangle
        {
            get { return _DisplayRectangle; }
            set
            {
                _DisplayRectangle.Size = value.Size;
                _Size = DisplayRectangle.Size;
            }
        }
        public Size Size
        {
            get { return _Size; }
            set
            {
                lock (this)
                {
                    _Size = new Size(
                        Math.Max(Math.Min(value.width, MaxSize.width), MinSize.width),
                        Math.Max(Math.Min(value.height, MaxSize.height), MinSize.height)
                        );

                    _DisplayRectangle.Size = new Size(Size.width, Size.height);
                    SizeChanged(_Size);
                }
            }
        }
        public event Action<Size> SizeChanged = delegate { };
        public event Action<Point> LocationChanged = delegate { };
        public Size MinSize = new Size(50, 50);
        public Size MaxSize = new Size(9000, 9000);

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="renderMethod">
        /// rednerMethod encapsulates the windowing and rendering system.
        /// variants use the same drawing implimnentations (although you will only see the udraw interface)
        /// so D2DLayered does the same drawing commands as D2DSwapChain, except it pushes the output of
        /// NoForm.DrawBase(and ofc children) to different places, uses different buffers and so on.
        /// </param>
        /// <param name="createOptions">
        /// encapsulates options that for whatever reason can only be set
        /// once.
        /// </param>
        public NoForm()
        {
            _components = new ComponentCollection(this);
            background = new USolidBrush() { color = new Color(1) };
        }
        public UBrush background;


            theForm.MouseDown += new MouseEventHandler(MouseDown);
            theForm.MouseUp += new MouseEventHandler(MouseUp);
            theForm.MouseMove += new MouseEventHandler(MouseMove);
            theForm.KeyDown += new KeyEventHandler((Object o, KeyEventArgs e) => { KeyUpDown(e.KeyCode, true); });
            theForm.KeyUp += new KeyEventHandler((Object o, KeyEventArgs e) => { KeyUpDown(e.KeyCode, false); });
            theForm.KeyPress += new KeyPressEventHandler((Object o, KeyPressEventArgs e) => { KeyPress(e.KeyChar); });

        // Key Events
        public void KeyUpDown(System.Windows.Forms.Keys key, bool keyDown)
        {
            foreach (IComponent inc in components)
                inc.KeyUpDown(key, keyDown);
        }
        public void KeyPress(char c)
        {
            foreach (IComponent inc in components)
                inc.KeyPress(c);
        }

        // Click Controller
        void MouseDown(Object o, MouseEventArgs mea)
        {
            MouseUpDown(mea, MouseButtonState.DOWN);
        }
        void MouseUp(Object o, MouseEventArgs mea)
        {
            MouseUpDown(mea, MouseButtonState.UP);
        }
        public void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs)
        {
            MouseButton mb;
            if (mea.Button == MouseButtons.Left) mb = MouseButton.LEFT;
            else if (mea.Button == MouseButtons.Right) mb = MouseButton.RIGHT;
            else return;

            var ceventargs = new ClickedEventArgs()
            {
                button = mb,
                state = mbs,
                clickLocation = mea.Location
            };

            if (Clicked != null)
                foreach (ClickedEventHandler cevent in Clicked.GetInvocationList())
                    cevent(ceventargs);
            foreach (IComponent inc in components)
                if (inc.visible)
                    inc.MouseUpDown(mea, mbs, Util.CursorInRect(inc.DisplayRectangle, Location), !Util.CursorInRect(DisplayRectangle, Location));
        }
        public struct ClickedEventArgs
        {
            public MouseButtonState state;
            public System.Drawing.Point clickLocation;
            public MouseButton button;
        }
        public delegate void ClickedEventHandler(ClickedEventArgs cea);
        public event ClickedEventHandler Clicked;

        // move contoller
        public delegate void MouseMoveEventHandler(System.Drawing.Point location);
        public event MouseMoveEventHandler MouseMoved;
        void MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove(e.Location);
        }
        public void MouseMove(System.Drawing.Point location)
        {
            foreach (IComponent inc in components)
            {
                bool clip = !Util.PointInRect(location, DisplayRectangle);
                bool inComponent = Util.PointInRect(location, inc.DisplayRectangle);
                if (inc.visible) inc.MouseMove(location, inComponent, clip);
            }

            if (Util.AmITopZOrder(this, location))
                window.Cursor = Cursor;

            if (MouseMoved != null) 
                foreach (MouseMoveEventHandler mevent in MouseMoved.GetInvocationList())
                    mevent(location);
        }

        public void DrawBase(IDraw rt)
        {
            rt.uDraw.Clear(new Color(0, 0, 0, 0)); // this lets alphas to desktop happen.
            rt.uDraw.FillRectangle(DisplayRectangle, background);

            Draw(rt);

            // Now we need to draw our childrens....
            rt.uDraw.PushAxisAlignedClip(DisplayRectangle,false);
            foreach (IComponent c in components)
                if (c.visible) c.DrawBase(rt);
            rt.uDraw.PopAxisAlignedClip();
        }
        public virtual void Draw(IDraw rt)
        {
        }

        public IWindow window;

        public void Create(IRender renderMethod, CreateOptions createOptions)
        {
            this.renderMethod = renderMethod;
            window = renderMethod.Init(this, createOptions);
        }

        // FIXME some isp could avoid this and keep the hierachy intact..
        public bool visible
        {
            get { return true; }
            set { throw new NotImplementedException(); }
        }
        public void RecalculateDisplayRectangle() { throw new NotImplementedException(); }
        public void RecalculateLocation() { throw new NotImplementedException(); }
        public void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped) { throw new NotImplementedException(); }
        public void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped) { throw new NotImplementedException(); }
        
    }
}
