using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Common;

namespace NoForms.Controls.Abstract
{
    public abstract class BasicContainer : Component
    {
        protected ComponentCollection _components;
        public override ComponentCollection components
        {
            get { return _components; }
        }
        public BasicContainer()
        {
            _components = new ComponentCollection(this);
        }

        // NOTE: No need to VisibilityChain in these events, because they terminate from the base.
        public override void DrawBase(IDraw renderArgument)
        {
            Draw(renderArgument);
            renderArgument.uDraw.PushAxisAlignedClip(DisplayRectangle,false);
            foreach (IComponent c in components)
                if (c.visible)
                    c.DrawBase(renderArgument);
            renderArgument.uDraw.PopAxisAlignedClip();
        }
        public abstract void Draw(IDraw renderArgument);

        public override void MouseMove(Point location, bool inComponent, bool amClipped)
        {
            base.MouseMove(location, inComponent, amClipped);
            foreach (IComponent c in components)
            {
                if (c.visible)
                {
                    bool child_inComponent = Util.PointInRect(location, c.DisplayRectangle);
                    bool child_amClipped = amClipped ? true : !Util.PointInRect(location, DisplayRectangle);
                    c.MouseMove(location, child_inComponent, child_amClipped);
                }
            }
        }
        public override void MouseUpDown(Point location,MouseButton mb,  Common.ButtonState bs, bool inComponent, bool amClipped)
        {
            base.MouseUpDown(location, mb, bs, inComponent, amClipped);
            foreach (IComponent c in components)
            {
                if (c.visible)
                    c.MouseUpDown(location, mb, bs, Util.PointInRect(location, c.DisplayRectangle),
                        amClipped ? true : !Util.PointInRect(location, DisplayRectangle));
            }
        }
        public override void KeyUpDown(System.Windows.Forms.Keys key, Common.ButtonState bs)
        {
            foreach (IComponent inc in components)
                inc.KeyUpDown(key, bs);
        }
        public override void KeyPress(char c)
        {
            foreach (IComponent inc in components)
                inc.KeyPress(c);
        }
    }
}
