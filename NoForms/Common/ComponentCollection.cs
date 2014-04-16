using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;

namespace NoForms
{
    // I do so little. FIXME what are the implications of using new vs virtial/override or a common base class/interface
    public class AlwaysEmptyComponentCollection : ComponentCollection
    {
        public AlwaysEmptyComponentCollection(IComponent dontCare) : base(null) { }
        public new bool Contains(IComponent item) { return false; }
        public new void Add(IComponent item) { }
        public new void Push(IComponent item) { }
        public new bool Remove(IComponent item) { return false; }
        public new bool RemoveAt(int idx) { return false; }
        public new void Clear() { }
        public new int IndexOf(IComponent ic) { return -1; }
        public new bool IsReadOnly { get { return true; } }
        public new void CopyTo(IComponent[] arr, int idx) { }
        public new IEnumerator<IComponent> GetEnumerator() { yield break; }
        public new IComponent this[int i] { get { return null; } }
    }

    public class ComponentCollection : ICollection<IComponent>
    {
        IComponent myParent;
        public ComponentCollection(IComponent myParent)
        {
            this.myParent = myParent;
        }

        public event Action<IComponent> ComponentAdded = delegate { };
        public event Action<IComponent> ComponentRemoved = delegate { };

        Collection<IComponent> back = new Collection<IComponent>();
        public bool Contains(IComponent item)
        {
            return back.Contains(item);
        }
        public void Add(IComponent item) { Add(item, false); }
        void Add(IComponent item, bool force)
        {
            lock (lo) back.Add(item);
            if (item.Parent != null)
                lock (item.Parent.components.lo)
                    item.Parent.components.Remove(item, force);
            item.Parent = myParent;
            item.RecalculateDisplayRectangle();
            ComponentAdded(item);
        }
        public void Push(IComponent item) { Push(item, false); }
        void Push(IComponent item, bool force)
        {
            lock (lo) back.Insert(0, item);
            if (item.Parent != null)
                lock (item.Parent.components.lo)
                    item.Parent.components.Remove(item, force);
            item.Parent = myParent;
            item.RecalculateDisplayRectangle();
            ComponentAdded(item);
        }
        public bool Remove(IComponent item) { return Remove(item, false); }
        bool Remove(IComponent item, bool force)
        {
            if (!Contains(item)) return false;
                    lock(lo) back.Remove(item);
                    item.Parent = null;
                    item.RecalculateDisplayRectangle();
                    ComponentRemoved(item);
            return true;
        }
        public bool RemoveAt(int index) { return RemoveAt(index, false); }
        bool RemoveAt(int index, bool force)
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
                foreach (IComponent t in back)
                    yield return t;
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
