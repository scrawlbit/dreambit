using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScrawlBit.Collections
{
    public class ExtendedObservableCollection<T> : ObservableCollection<T>, IObservableCollection<T>, IReadOnlyObservableCollection<T>
    {
        public ExtendedObservableCollection() { }
        public ExtendedObservableCollection(List<T> list) : base(list) { }
        public ExtendedObservableCollection(IEnumerable<T> collection) : base(collection) { }

        object IObservableCollection.this[int index] => this[index];
        void IObservableCollection.Insert(int index, object item)
        {
            Insert(index, (T)item);
        }
        void IObservableCollection.Add(object item)
        {
            Add((T)item);
        }
        bool IObservableCollection.Remove(object item)
        {
            return Remove((T)item);
        }
        int IObservableCollection.IndexOf(object item)
        {
            return IndexOf((T)item);
        }
    }
}