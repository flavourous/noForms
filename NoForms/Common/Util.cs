﻿using System;
using System.Collections.Generic;
using System.Text;
using SysRect = System.Drawing.Rectangle;

namespace NoForms
{
    public static class Util
    {
        /// <summary>
        /// The point loc must be relative to the top level component (NoForm)
        /// </summary>
        /// <param name="me"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static bool AmITopZOrder(IComponent me, System.Drawing.Point loc)
        {
            if (me.Parent == null && !(me is NoForm))
                throw new Exception("How the fuck am I supposed to know?");

            // assume true to start with, then check

            // 0) children, need to check that none are at this point...
            // 1) get parent, if null, return, otherwise..
            // 2) siblings, need to check for any overlap of ones with higer zorder at this point
            // 3) go back to 1

            // (0)
            if (me is IComponent)
                foreach (var ic in (me as IComponent).components)
                    if (PointInRect(loc, ic.DisplayRectangle) && ic.visible)
                        return false;
        
            int myIdx;
            myIdx = me.Parent.components.IndexOf(me);

            //(2) for me which might not be a container...
            if (myIdx < me.Parent.components.Count - 1)
                for (int i = myIdx + 1; i < me.Parent.components.Count; i++)
                    if (PointInRect(loc, me.Parent.components[i].DisplayRectangle) && me.Parent.components[i].visible)
                        return false;

            IComponent par = me.Parent;
            while (true)
            {
                if (par.Parent == null) break; // (1)
                myIdx = par.Parent.components.IndexOf(par as IComponent);

                //(2)
                if (myIdx < par.Parent.components.Count - 1)
                    for (int i = myIdx + 1; i < par.Parent.components.Count; i++)
                        if (PointInRect(loc, par.Parent.components[i].DisplayRectangle) && par.Parent.components[i].visible)
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
        public static Point GetTopLevelLocation(IComponent inc)
        {
            IComponent par = inc.Parent;
            while (par.Parent != null)
                par = par.Parent;
            return par.Location;
        }
    }
}
