using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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
        
        Collection<IComponent> back = new Collection<IComponent>();
        List<DelayedProc> dprocs = new List<DelayedProc>();
        public bool Contains(IComponent item)
        {
            return back.Contains(item);
        }
        public void Add(IComponent item) { Add(item, false); }
        void Add(IComponent item, bool force)
        {
            if (!enumerating || force)
            {
                back.Add(item);
                if (item.Parent != null)
                    item.Parent.components.Remove(item,force);
                item.Parent = myParent;
                item.RecalculateDisplayRectangle();
            }
            else dprocs.Add(new DelayedProc(item, CCAction.Add));
        }
        public void Push(IComponent item) { Push(item, false); }
        void Push(IComponent item, bool force)
        {
            if (!enumerating || force)
            {
                back.Insert(0, item);
                if (item.Parent != null)
                    item.Parent.components.Remove(item,force);
                item.Parent = myParent;
                item.RecalculateDisplayRectangle();
            }
            else dprocs.Add(new DelayedProc(item, CCAction.Push));
        }
        public bool Remove(IComponent item) { return Remove(item, false); }
        bool Remove(IComponent item, bool force)
        {
            if (!Contains(item)) return false;
            else
            {
                if (!enumerating || force)
                {
                    back.Remove(item);
                    item.Parent = null;
                    item.RecalculateDisplayRectangle();
                }
                else dprocs.Add(new DelayedProc(item, CCAction.Remove));
            }
            return true;
        }
        public bool RemoveAt(int index) { return RemoveAt(index, false); }
        bool RemoveAt(int index, bool force)
        {
            if (back.Count <= index) return false;
            else
            {
                var item = back[index];
                if (!enumerating || force)
                {
                    back.Remove(item);
                    item.Parent = null;
                    item.RecalculateDisplayRectangle();
                }
                else dprocs.Add(new DelayedProc(item, CCAction.Remove));
            }
            return true;
        }
        public void Clear()
        {
            if (!enumerating) back.Clear();
            else dprocs.Add(new DelayedProc(null, CCAction.Clear));
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
        bool enumerating = false;
        object lo = new object();
        public IEnumerator<IComponent> GetEnumerator()
        {
            enumerating = true;
            lock (lo)
            {
                foreach (IComponent t in back)
                    yield return t;
                while (dprocs.Count > 0)
                {
                    var dp = dprocs[0];
                    switch (dp.act)
                    {
                        case (CCAction.Add): Add(dp.cref,true); break;
                        case (CCAction.Push): Push(dp.cref,true); break;
                        case (CCAction.Remove): Remove(dp.cref,true); break;
                        case (CCAction.Clear): Clear(); break;
                    }
                    dprocs.RemoveAt(0);
                }
            }
            enumerating = false;
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
    enum CCAction { Add, Remove, Push, Clear };
    struct DelayedProc
    {
        public IComponent cref;
        public CCAction act;
        public DelayedProc(IComponent ic, CCAction ca)
        {
            cref = ic;
            act = ca;
        }
    }
}
