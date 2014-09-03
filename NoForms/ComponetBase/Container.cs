using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NoForms.Common;

namespace NoForms.ComponentBase
{
    public abstract class Container : Component
    {
        protected IComponent_Collection _components;
        public override IComponent_Collection components
        {
            get { return _components; }
        }
        public Container()
        {
            _components = new IComponent_Collection(this);
        }

        // NOTE: No need to VisibilityChain in these events, because they terminate from the base.
        public override void DrawBase(IDraw renderArgument, Region dirty)
        {
            Draw(renderArgument, dirty);
            foreach (IComponent c in components)
                if (c.visible && dirty.Intersects(c.DisplayRectangle))
                    c.DrawBase(renderArgument, dirty);
        }
        public abstract void Draw(IDraw renderArgument, Region dirty);

        public override void MouseMove(Point location, bool inComponent, bool amClipped)
        {
            base.MouseMove(location, inComponent, amClipped);
            foreach (IComponent c in components)
            {
                if (c.visible)
                {
                    bool child_inComponent = c.DisplayRectangle.Contains(location);
                    bool child_amClipped = amClipped ? true : !DisplayRectangle.Contains(location);
                    c.MouseMove(location, child_inComponent, child_amClipped);
                }
            }
        }
        public override void MouseUpDown(Point location,MouseButton mb,  NoForms.Common.ButtonState bs, bool inComponent, bool amClipped)
        {
            base.MouseUpDown(location, mb, bs, inComponent, amClipped);
            foreach (IComponent c in components)
            {
                if (c.visible)
                    c.MouseUpDown(location, mb, bs, c.DisplayRectangle.Contains(location),
                        amClipped ? true : !DisplayRectangle.Contains(location));
            }
        }
        public override void KeyUpDown(NoForms.Common.Keys key, NoForms.Common.ButtonState bs)
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
