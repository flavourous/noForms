using System;
using System.Collections.Generic;
using NoForms.Common;

namespace NoForms
{
    public class FocusManager
    {
        static FocusManager empty = new EmptyFocusManager();
        public static FocusManager Empty { get { return empty; } }

        Object focusLock = new object();
        IComponent focused;
        public virtual void FocusSet(IComponent setFocus, bool focus)
        {
            lock (focusLock)
            {
                if (focused == setFocus && !focus)
                    focused = null;
                else if(focus)
                    focused = setFocus;
            }
        }
        public virtual bool FocusGet(IComponent getFocus)
        {
            lock(focusLock)
                return object.ReferenceEquals(getFocus, focused);
        }
    }
    public class EmptyFocusManager : FocusManager
    {
        public override bool FocusGet(IComponent getFocus) { return false; }
        public override void FocusSet(IComponent setFocus, bool focus) { }
    }


    // FIXME think about how to get one of these to work, espically with things like the scrollcontainer...
    interface IHitInfo
    {
        bool inComponent(IComponent c);  // simply if you're in the region (recangle)
        bool amClipped(IComponent c);    // are you clipped by your parent? (maybe by self at some futer point too - we have Icomponent.visble right now, but if false the component never recieves the hit)
        bool amTopVisible(IComponent c); // of non-clipped elements at this position, are you on top in terms of ZOrder?
    }
    /// <summary>
    /// My Prediction - this stuff will always have bugs! go find em!
    /// </summary>
    class HitTestManager : IHitInfo
    {
        // When accessed, need to do a traceback along IComponent.Parent.  And cache the result.  This is the managers price.
        // Cache should be invalidated on each hit event, provide method for that.
        bool hitting = false;
        Point hitPoint = Point.Zero;

        // The old question.  I went for seperate caches, we're already using memory to increase performance (caching), lets just keep doing that (until we hit a problem).
        Dictionary<IComponent, bool> inComponentCache = new Dictionary<IComponent, bool>();
        Dictionary<IComponent, bool> amClippedCache = new Dictionary<IComponent, bool>();
        Dictionary<IComponent, bool> amTopVisibleCache = new Dictionary<IComponent, bool>();
        public void HitStarted(Point hit)
        {
            if (hitting)
            {
                throw new NotImplementedException("Do not support multiple concurrent hits yet.");
            }
            hitting = true;
            hitPoint = hit;
        }
        public void HitFinished()
        {
            hitting = false;
            inComponentCache.Clear();
            amClippedCache.Clear();
            amTopVisibleCache.Clear();
            hitPoint = Point.Zero;
        }

        // These must be called with a component.  Annoying but cant think of a way to do from property that doesnt suck....
        public bool inComponent(IComponent c)
        {
            // Very Simple:
            if (!inComponentCache.ContainsKey(c))
                inComponentCache[c] =  c.DisplayRectangle.Contains(hitPoint);
            return inComponentCache[c];
        }
        public bool amClipped(IComponent c)
        {
            // If we're cached, just return it
            if (amClippedCache.ContainsKey(c))
                return amClippedCache[c];

            // if the parent (or any parent) is clipped at this point, so are you, so dont bother.
            // update caches for every parent we have to end up evaluating this for.
            bool parentIsClipped = false;
            if (c.Parent != null) parentIsClipped = amClipped(c.Parent);

            // if parent is clipped at this point, so are you my child.
            if (parentIsClipped) 
                return amClippedCache[c] = true;

            // Otherwise we actually get to do the deliscious hit testing...
            return amClippedCache[c] = inComponent(c); //... alright, clipping is damn simple right now.
            // It goes without saying that when or if a clipToBounds and/or clipRegion property is implimented, this will need changing?
        }
        public bool amTopVisible(IComponent c)
        {
            throw new NotImplementedException();
        }
    }
    
}
