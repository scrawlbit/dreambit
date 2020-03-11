using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Scrawlbit.Helpers
{
    public static class CollectionHelper
    {
        public static T Pop<T>(this ICollection<T> collection)
        {
            var item = collection.Last();
            collection.Remove(item);

            return item;
        }
        public static bool Pop<T>(this ICollection<T> collection, out T item)
        {
            item = collection.LastOrDefault();
            return collection.Remove(item);
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }
        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> confirm)
        {
            var copy = collection.ToArray();

            foreach (var item in copy)
            {
                if (confirm(item))
                    collection.Remove(item);
            }
        }
    }
}