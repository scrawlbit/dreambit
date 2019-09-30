using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ScrawlBit.Notification;

namespace ScrawlBit.Collections
{
    public abstract class ReadOnlyObservableCollectionWrapper<T> : NotificationObject, IReadOnlyObservableCollection<T>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => Collection.CollectionChanged += value;
            remove => Collection.CollectionChanged -= value;
        }
        public override event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                base.PropertyChanged += value;
                Collection.PropertyChanged += value;
            }
            remove
            {
                base.PropertyChanged -= value;
                Collection.PropertyChanged -= value;
            }
        }
        protected abstract IReadOnlyObservableCollection<T> Collection { get; }
        public int Count => Collection.Count;
        public T this[int index] => Collection[index];

        public IEnumerator<T> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}