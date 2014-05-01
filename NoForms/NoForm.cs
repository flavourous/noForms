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
    }
    /// <summary>
    /// You would create a noform, and to it add INoComponent, and it would handle stuff.
    /// </summary>
    public class NoForm : IComponent
    {
        // FIXME does this need IDisposable? :/  dont wana dispose renderMethod really might use elsewhere? certainly for combobox..
        public IComponent Parent { get { return null; } set { } }
        ComponentCollection _components;
        public ComponentCollection components
        {
            get { return _components; }
        }
        public int ZIndex { get { return 0; } }
        public event Action<IComponent> ZIndexChanged = delegate { };

        protected IRender renderMethod;
        internal Form theForm;

        public Cursor Cursor { get; set; }
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

        public System.Drawing.Icon icon = null;
        public String title = "";

        public NoForm(IRender renderMethod, CreateOptions createOptions = null)
        {
            this.createOptions = createOptions ?? new CreateOptions();
            this.renderMethod = renderMethod;
            renderMethod.noForm = this;
            _components = new ComponentCollection(this);
            background = new USolidBrush() { color = new Color(1) };
        }
        public UBrush background;


        // Register controller events
        internal void RegisterControllers()
        {
            theForm.MouseDown += new MouseEventHandler(MouseDown);
            theForm.MouseUp += new MouseEventHandler(MouseUp);
            theForm.MouseMove += new MouseEventHandler(MouseMove);
            theForm.KeyDown += new KeyEventHandler((Object o, KeyEventArgs e) => { KeyUpDown(e.KeyCode, true); });
            theForm.KeyUp += new KeyEventHandler((Object o, KeyEventArgs e) => { KeyUpDown(e.KeyCode, false); });
            theForm.KeyPress += new KeyPressEventHandler((Object o, KeyPressEventArgs e) => { KeyPress(e.KeyChar); });
        }

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
                currentCursor = Cursor;

            if (MouseMoved != null) 
                foreach (MouseMoveEventHandler mevent in MouseMoved.GetInvocationList())
                    mevent(location);
        }

        public Cursor currentCursor
        {
            get { return theForm.Cursor; }
            set { theForm.Cursor = value; }
        }

        public void DrawBase(IRenderType rt)
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
        public virtual void Draw(IRenderType rt)
        {
        }

        // FIXME some of these cant be changed after!!
        void SetWFormProps()
        {
            theForm.Text = title;
            theForm.ShowIcon = icon != null;
            if (icon != null) theForm.Icon = icon;
        }

        CreateOptions createOptions;
        void OnceOnlyProperties()
        {
            theForm.ShowInTaskbar = createOptions.showInTaskbar;
        }

        bool okClose = false;
        public void Create(bool rootForm, bool dialog)
        {
            renderMethod.Init(ref theForm, OnceOnlyProperties);
            SetWFormProps();
            RegisterControllers();

            theForm.Load += new EventHandler((object o, EventArgs e) =>
            {
                renderMethod.BeginRender();
            });
            theForm.FormClosing += new FormClosingEventHandler((object o, FormClosingEventArgs e) =>
            {
                if (!okClose) e.Cancel = true;
                renderMethod.EndRender(new MethodInvoker(() => Close(true)));
            });

            if (rootForm)
            {
                Application.EnableVisualStyles();
                Application.Run(theForm);
            }
            else
            {
                if (dialog) theForm.ShowDialog();
                else theForm.Show();
            }
        }
        
        public void Close(bool done = false)
        {
            if (theForm.InvokeRequired)
            {
                theForm.Invoke(new MethodInvoker(() => Close(done)));
                return;
            }
            okClose = done;
            theForm.Close();
        }

        public void BringToFront()
        {
            theForm.BringToFront();
        }

        WindowState _windowState = WindowState.Normal;
        public WindowState windowState
        {
            get { return _windowState; }
            set { ChangeWindowState(value); }
        }
        void ChangeWindowState(WindowState ws)
        {
            if (ws == _windowState) return;
            switch (ws)
            {
                case WindowState.Minimized:
                    theForm.WindowState = FormWindowState.Minimized;
                    break;
                case WindowState.Maximised:
                    theForm.WindowState = FormWindowState.Maximized;
                    break;
                case WindowState.Normal:
                    theForm.WindowState = FormWindowState.Normal;
                    break;
            }
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
