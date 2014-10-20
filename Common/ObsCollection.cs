using System;
using System.Collections.Generic;
using System.Text;

namespace NoForms.Common
{
    public delegate void VoidAction();
    public interface IObservable
    {
        event VoidAction changed;
    }
    public class ObsCollection<T> : System.Collections.ObjectModel.Collection<T>, IObservable
    {
        public event VoidAction changed;
        void OnCollectionChanged() 
        {
            if(changed != null)
                changed();
        }
        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged();
        }
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            if(item is IObservable)
                (item as IObservable).changed += OnCollectionChanged;
            OnCollectionChanged();
        }
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            OnCollectionChanged();
        }
        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            OnCollectionChanged();
            if (item is IObservable)
                (item as IObservable).changed += OnCollectionChanged;
        } 
    }
}
