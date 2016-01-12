namespace SDTD.Config
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a collection of XML elements (<c>IElement</c>).
    /// </summary>
    interface IElementCollection
    {
        void Load(XDocument document);
        void Save(XDocument document);
    }

    /// <summary>
    /// Represents an observable, indexed collection of observable elements.
    /// </summary>
    /// <typeparam name="T">The type of element in the collection.</typeparam>
    public class ObservableBaseCollection<T> : KeyedCollection<String, T>, INotifyCollectionChanged
        where T : ObservableBase<T>
    {
        protected static String _elementName = typeof(T).Name.ToLowerInvariant();

        protected ObservableBaseCollection() : base(null, 0)
        {
            // No additional implementation.
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public Boolean ContainsKey(String key)
        {
            return this.Dictionary.Keys.Contains(key);
        }

        internal void UpdateKey(T item)
        {
            base.ChangeItemKey(item, this.GetKeyForItem(item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override String GetKeyForItem(T item)
        {
            return item.Name;
        }

        protected override void InsertItem(Int32 index, T item)
        {
            base.InsertItem(index, item);
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(Int32 index)
        {
            T removedItem = this[index];
            base.RemoveItem(index);
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        protected override void SetItem(Int32 index, T item)
        {
            T oldItem = this[index];
            base.SetItem(index, item);
            this.CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }
    }
}
