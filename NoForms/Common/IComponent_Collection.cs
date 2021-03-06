﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;

namespace NoForms
{
    // I do so little. FIXME what are the implications of using new vs virtial/override or a common base class/interface
    public class IComponent_Collection_AlwaysEmpty : IComponent_Collection
    {
        public IComponent_Collection_AlwaysEmpty(IComponent dontCare) : base(null) { }
        public new bool Contains(IComponent item) { return false; }
        public new void Add(IComponent item) { }
        public new bool Remove(IComponent item) { return false; }
        public new bool RemoveAt(int idx) { return false; }
        public new void Clear() { }
        public new int IndexOf(IComponent ic) { return -1; }
        public new bool IsReadOnly { get { return true; } }
        public new void CopyTo(IComponent[] arr, int idx) { }
        public new IEnumerator<IComponent> GetEnumerator() { yield break; }
        public new IComponent this[int i] { get { return null; } }
    }

    public class IComponent_Collection : ICollection<IComponent>
    {
        IComponent myParent;
        public IComponent_Collection(IComponent myParent)
        {
            this.myParent = myParent;

            // Preserving zorder
            ComponentAdded += TrackZIndex_Add;
            ComponentRemoved += TrackZIndex_Remove;
        }

        // FIXME some performance to be gained here
        void TrackZIndex_Add(IComponent obj)
        {
            obj.ZIndexChanged += obj_ZIndexChanged;
            obj_ZIndexChanged(obj);
        }
        void obj_ZIndexChanged(IComponent obj)
        {
            lock (lo)
            {
                back.Remove(obj);
                int i = 0;
                for (i = 0; i < back.Count; i++)
                    if (back[i].ZIndex > obj.ZIndex)
                        break;
                back.Insert(i, obj);
            }
        }
        void TrackZIndex_Remove(IComponent obj)
        {
            obj.ZIndexChanged -= obj_ZIndexChanged;
        }

        public event Action<IComponent> ComponentAdded = delegate { };
        public event Action<IComponent> ComponentRemoved = delegate { };

        protected Collection<IComponent> back = new Collection<IComponent>();
        public bool Contains(IComponent item)
        {
            return back.Contains(item);
        }
        public virtual void Add(IComponent item)
        {
            lock (lo) back.Add(item);
            if (item.Parent != null)
                lock (item.Parent.components.lo)
                    item.Parent.components.Remove(item);
            item.Parent = myParent;
            item.RecalculateDisplayRectangle();
            ComponentAdded(item);
        }
        public bool Remove(IComponent item)
        {
            if (!Contains(item)) return false;
                    lock(lo) back.Remove(item);
                    item.Parent = null;
                    item.RecalculateDisplayRectangle();
                    ComponentRemoved(item);
            return true;
        }
        public bool RemoveAt(int index)
        {
            if (back.Count <= index) return false;
                var item = back[index];
                    lock(lo) back.Remove(item);
                    item.Parent = null;
                    item.RecalculateDisplayRectangle();
                    ComponentRemoved(item);
            return true;
        }
        public void Clear()
        {
            Collection<IComponent> remember = back;
            lock (lo) back.Clear();
            foreach (var c in remember) 
                ComponentRemoved(c);
        }
        public int IndexOf(IComponent ic)
        {
            return back.IndexOf(ic);
        }
        public int Count { get { return back.Count; } }
        public bool IsReadOnly { get { return false; } }
        public void CopyTo(IComponent[] arr, int idx)
        {
            back.CopyTo(arr, idx);
        }
        object lo = new object();
        public IEnumerator<IComponent> GetEnumerator()
        {
            lock (lo)
            {
                for (int i = 0; i < back.Count; i++) 
                    yield return back[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock(lo)
                return GetEnumerator();
        }
        public IComponent this[int i]
        {
            get { return back[i]; }
        }
    }
}
