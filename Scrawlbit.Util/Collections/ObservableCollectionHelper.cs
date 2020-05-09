using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scrawlbit.Helpers;

namespace Scrawlbit.Collections
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

        public static void OrderIndexes<T>(this IObservableCollection<T> collection, IEnumerable<T> reference)
        {
            T[] itens = reference.ToArray();

            for (int i = 0; i < itens.Length; i++)
            {
                int oldIndex = collection.IndexOf(itens[i]);
                int newIndex = i;

                collection.Move(oldIndex, newIndex);
            }
        }

        public static ExtendedObservableCollection<T> ToObservable<T>(this IEnumerable<T> source)
        {
            return new ExtendedObservableCollection<T>(source);
        }
    }
}