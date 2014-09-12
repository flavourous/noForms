using System;
using System.Collections.Generic;
using System.Text;
using NoForms.Common;

namespace NoForms
{
    public static class IComponent_Util
    {
        public static void OnAllChildren(IComponent root, Action<IComponent> visit)
        {
            foreach (var c in AllChildren(root))
                visit(c);
        }
        public static IEnumerable<IComponent> AllChildren(IComponent root)
        {
            foreach (var c in root.components)
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                    yield return cc;
            }
        }
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

        public static bool AmITopZOrder(IComponent me, Point loc)
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
                    if ( ic.DisplayRectangle.Contains(loc) && VisibilityChain(ic))
                        return false;


            if (me.Parent == null) return true;

            int myIdx;
            myIdx = me.Parent.components.IndexOf(me);

            //(2) for me which might not be a container...
            if (myIdx < me.Parent.components.Count - 1)
                for (int i = myIdx + 1; i < me.Parent.components.Count; i++)
                    if ( me.Parent.components[i].DisplayRectangle.Contains(loc) && VisibilityChain(me.Parent.components[i]))
                        return false;

            IComponent par = me.Parent;
            while (true)
            {
                if (par.Parent == null) break; // (1)
                myIdx = par.Parent.components.IndexOf(par as IComponent);

                //(2)
                if (myIdx < par.Parent.components.Count - 1)
                    for (int i = myIdx + 1; i < par.Parent.components.Count; i++)
                        if ( par.Parent.components[i].DisplayRectangle.Contains(loc) && VisibilityChain(par.Parent.components[i]))
                            return false;

                par = par.Parent;
            } // (3)

            return true;
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
