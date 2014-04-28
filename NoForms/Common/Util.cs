using System;
using System.Collections.Generic;
using System.Text;
using SysRect = System.Drawing.Rectangle;

namespace NoForms
{
    public static class Util
    {
        /// <summary>
        /// If self and all parent chain is visible
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static bool VisibilityChain(IComponent me)
        {
            IComponent curr = me;
            while (curr != null)
            {
                if (!curr.visible) return false;
                curr = curr.Parent;
            }
            return true;
        }

        public static bool AmITopZOrder(IComponent me, System.Drawing.Point loc)
        {
            if (me.Parent == null && !(me is NoForm))
                throw new Exception("How the fuck am I supposed to know?");

            //oh yea...
            if (!VisibilityChain(me)) return false;

            // assume true to start with, then check

            // 0) children, need to check that none are at this point...
            // 1) get parent, if null, return, otherwise..
            // 2) siblings, need to check for any overlap of ones with higer zorder at this point
            // 3) go back to 1

            // (0)
            if (me is IComponent)
                foreach (var ic in (me as IComponent).components)
                    if (PointInRect(loc, ic.DisplayRectangle) && VisibilityChain(ic))
                        return false;
        
            int myIdx;
            myIdx = me.Parent.components.IndexOf(me);

            //(2) for me which might not be a container...
            if (myIdx < me.Parent.components.Count - 1)
                for (int i = myIdx + 1; i < me.Parent.components.Count; i++)
                    if (PointInRect(loc, me.Parent.components[i].DisplayRectangle) && VisibilityChain(me.Parent.components[i]))
                        return false;

            IComponent par = me.Parent;
            while (true)
            {
                if (par.Parent == null) break; // (1)
                myIdx = par.Parent.components.IndexOf(par as IComponent);

                //(2)
                if (myIdx < par.Parent.components.Count - 1)
                    for (int i = myIdx + 1; i < par.Parent.components.Count; i++)
                        if (PointInRect(loc, par.Parent.components[i].DisplayRectangle) && VisibilityChain(par.Parent.components[i]))
                            return false;

                par = par.Parent;
            } // (3)

            return true;
        }
        public static bool CursorInRect(Rectangle dr, Point topLevelLocation)
        {
            Point ccl = System.Windows.Forms.Cursor.Position;
            ccl.X -= topLevelLocation.X;
            ccl.Y -= topLevelLocation.Y;
            return PointInRect(ccl, dr);
        }
        public static bool PointInRect(Point ccl, Rectangle dr)
        {
            if (ccl.X >= dr.left && ccl.X <= dr.right)
                if (ccl.Y >= dr.top && ccl.Y <= dr.bottom)
                    return true;
            return false;
        }
        public static IComponent GetTopLevelComponent(IComponent inc)
        {
            IComponent par = inc;
            while (par.Parent != null)
                par = par.Parent;
            return par;
        }
        public static Point GetTopLevelLocation(IComponent inc)
        {
            var par = GetTopLevelComponent(inc);
            return par.Location;
        }
        public static Point GetTopLevelLocation(IComponent inc, out IComponent topLevelComponent)
        {
            var par = GetTopLevelComponent(inc);
            topLevelComponent = par;
            return par.Location;
        }
    }
}
