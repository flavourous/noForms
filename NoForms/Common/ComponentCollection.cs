using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace NoForms
{
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
