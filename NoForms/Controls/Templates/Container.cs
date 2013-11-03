using System;
using System.Windows.Forms;
using SysRect = System.Drawing.Rectangle;

namespace NoForms.Controls.Templates
{
    public abstract class Container : Containable, IContainer
    {
        ComponentCollection _components;
        public ComponentCollection components
        {
            get { return _components; }
        }

        public Container()
        {
            _components = new ComponentCollection(this);
        }

        public override void RecalculateDisplayRectangle()
        {
            base.RecalculateDisplayRectangle();
            foreach (IComponent c in components)
                c.RecalculateDisplayRectangle();
        }
        public override void RecalculateLocation()
        {
            base.RecalculateLocation();
            foreach (IComponent c in components)
                c.RecalculateLocation();
        }

        Rectangle clipSet = Rectangle.Empty;
        public void UnClipAll<RenderType>(RenderType renderArgument)
        {
            Util.SetClip<RenderType>(renderArgument, false, Rectangle.Empty);
            Parent.UnClipAll<RenderType>(renderArgument);
        }
        public void ReClipAll<RenderType>(RenderType renderArgument)
        {
            Parent.ReClipAll<RenderType>(renderArgument);
            Util.SetClip<RenderType>(renderArgument, true, clipSet);
        }


        /// <summary>
        /// You MUST call this base method when you override, at the end of your method, to
        /// draw the children.
        /// </summary>
        /// <typeparam name="RenderType"></typeparam>
        /// <param name="renderArgument"></param>
        /// <param name="parentDisplayRectangle"></param>
        public override void DrawBase<RenderType>(RenderType renderArgument)
        {
            Util.SetClip<RenderType>(renderArgument, true, clipSet = DisplayRectangle);
            foreach (IComponent c in components)
                if (c.visible)
                    c.DrawBase<RenderType>(renderArgument);
            Util.SetClip<RenderType>(renderArgument, false, clipSet = Rectangle.Empty);
        }
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
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
        public override void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            foreach (IComponent c in components)
            {
                if (c.visible)
                    c.MouseUpDown(mea, mbs, Util.CursorInRect(c.DisplayRectangle, Util.GetTopLevelLocation(c)), 
                        amClipped ? true : !Util.CursorInRect(DisplayRectangle, Util.GetTopLevelLocation(this)));
            }
        }
        public override void KeyDown(System.Windows.Forms.Keys key)
        {
            foreach (IComponent inc in components)
                if (inc is Focusable)
                    (inc as Focusable).KeyDown(key);
        }
        public override void KeyUp(System.Windows.Forms.Keys key)
        {
            foreach (IComponent inc in components)
                if (inc is Focusable)
                    (inc as Focusable).KeyUp(key);
        }
        public override void KeyPress(char c)
        {
            foreach (IComponent inc in components)
                if (inc is Focusable)
                    (inc as Focusable).KeyPress(c);
        }
    }
}
