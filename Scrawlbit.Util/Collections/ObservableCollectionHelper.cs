using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScrawlBit.Helpers;

namespace ScrawlBit.Collections
{
    public static class ObservableCollectionHelper
    {
        public static void AddRange(this IObservableCollection collection, IEnumerable items)
        {
            items.ForEach(collection.Add);
        }
        public static void RemoveRange(this IObservableCollection collection, IEnumerable items)
        {
            items.ForEach(i => collection.Remove(i));
        }
        public static void RemoveAll(this IObservableCollection collection, Func<object, bool> confirm)
        {
            var items = collection.Cast<object>().Where(confirm).ToArray();

            collection.RemoveRange(items);
        }

        public static void AddRange<T>(this IObservableCollection<T> collection, IEnumerable<T> items)
        {
            items.ForEach(collection.Add);
        }
        public static void RemoveRange<T>(this IObservableCollection<T> collection, IEnumerable<T> items)
        {
            items.ForEach(i => collection.Remove(i));
        }

        public static ExtendedObservableCollection<T> ToObservable<T>(this IEnumerable<T> source)
        {
            return new ExtendedObservableCollection<T>(source);
        }
    }
}