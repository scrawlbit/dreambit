using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Scrawlbit.Collections
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

    public interface IObservableCollection<T> : IObservableCollection, IList<T>, IReadOnlyObservableCollection<T>
    {
        new int Count { get; }
        new T this[int index] { get; }

        new void Clear();
    }
}