using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ScrawlBit.Collections
{
    public interface IReadOnlyObservableCollection<out T> : IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        T this[int index] { get; }
    }
}