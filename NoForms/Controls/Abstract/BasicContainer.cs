using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

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
        public override void DrawBase(IRenderType renderArgument)
        {
            Draw(renderArgument);
            renderArgument.uDraw.PushAxisAlignedClip(DisplayRectangle,false);
            foreach (IComponent c in components)
                if (c.visible)
                    c.DrawBase(renderArgument);
            renderArgument.uDraw.PopAxisAlignedClip();
        }
        public abstract void Draw(IRenderType renderArgument);

        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
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
        public override void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            base.MouseUpDown(mea,mbs,inComponent,amClipped);
            foreach (IComponent c in components)
            {
                if (c.visible)
                    c.MouseUpDown(mea, mbs, Util.CursorInRect(c.DisplayRectangle, Util.GetTopLevelLocation(c)),
                        amClipped ? true : !Util.CursorInRect(DisplayRectangle, Util.GetTopLevelLocation(this)));
            }
        }
        public override void KeyUpDown(System.Windows.Forms.Keys key, bool keyDown)
        {
            foreach (IComponent inc in components)
                inc.KeyUpDown(key, keyDown);
        }
        public override void KeyPress(char c)
        {
            foreach (IComponent inc in components)
                inc.KeyPress(c);
        }
    }
}
