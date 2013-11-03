using System;
using System.Collections.Generic;
using System.Text;

namespace NoForms
{
    public interface IObservable
    {
        event System.Windows.Forms.MethodInvoker collectionChanged;
    }
    public class ObsCollection<T> : System.Collections.ObjectModel.Collection<T>, IObservable
    {
        public event System.Windows.Forms.MethodInvoker collectionChanged;
        void OnCollectionChanged() 
        {
            if(collectionChanged != null)
                collectionChanged();
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
                (item as IObservable).collectionChanged += OnCollectionChanged;
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
                (item as IObservable).collectionChanged += OnCollectionChanged;
        } 
    }
}
