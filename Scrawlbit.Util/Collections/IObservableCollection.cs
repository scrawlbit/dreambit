using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ScrawlBit.Collections
{
    public interface IObservableCollection : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        object this[int index] { get; }
        int Count { get; }

        void Insert(int index, object item);
        void Add(object item);
        bool Remove(object item);
        void Move(int oldIndex, int newIndex);

        int IndexOf(object item);

        void Clear();
    }

    public interface IObservableCollection<T> : IObservableCollection, ICollection<T>
    {
        new int Count { get; }
        new T this[int index] { get; }

        void Insert(int index, T item);
        int IndexOf(T item);

        new void Clear();
    }
}