using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrawlbit.Helpers
{
    public static class ListHelper
    {
        public static void InsertOrdered<T>(this IList<T> collection, T item, Func<IEnumerable<T>, IOrderedEnumerable<T>> ordering)
        {
            var items = ordering(collection.Union(item));
            var index = items.IndexOf(item);

            collection.Insert(index, item);
        }
        public static void InsertOrdered<T>(this IList<T> collection, T item, Func<T, object> orderBy, params Func<T, object>[] thenBy)
        {
            collection.InsertOrdered(item, c =>
            {
                var items = c.OrderBy(orderBy);

                foreach (var t in thenBy)
                    items = items.ThenBy(t);

                return items;
            });
        }
    }
}