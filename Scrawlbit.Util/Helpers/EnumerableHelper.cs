using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scrawlbit.Comparison;

namespace Scrawlbit.Helpers
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item)
        {
            return source.Except(new[] { item });
        }
        public static IEnumerable<T> Except<T, TComparer>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TComparer> comparer)
        {
            return first.Except(second, new FuncEqualityComparer<T, TComparer>(comparer));
        }

        public static IEnumerable<T> Union<T>(this IEnumerable<T> source, params T[] item)
        {
            return Enumerable.Union(source, item ?? new T[0]);
        }

        public static bool All<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            int index = 0;

            foreach (var item in source)
            {
                if (!predicate(item, index++))
                    return false;
            }

            return true;
        }

        public static void ForEach(this IEnumerable source, Action<object> action)
        {
            foreach (var item in source)
                action(item);
        }
        public static void ForEach<T>(this IEnumerable collection, Action<T> action)
        {
            foreach (var item in collection)
                action((T)item);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            var i = 0;
            foreach (var item in source)
                action(i++, item);
        }

        public static bool Contains(this IEnumerable source, object item)
        {
            return Enumerable.Contains(source.Cast<object>(), item);
        }
        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> items)
        {
            return items.Any(i => Enumerable.Contains(source, i));
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T item)
        {
            var index = 0;

            foreach (var i in source)
            {
                if (Equals(i, item))
                    return index;

                index++;
            }

            return -1;
        }

        public static bool IsAny<T>(this T item, params T[] items)
        {
            return items.Contains(item);
        }

        public static IEnumerable<T> NotNulls<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(i => !Equals(i, null));
        }
        public static IEnumerable<T> NotNulls<T>(this IEnumerable<T?> source) where T : struct
        {
            return source.Where(i => !Equals(i, null)).Cast<T>();
        }

        public static bool In<T>(this T item, params T[] itens)
        {
            return itens.Contains(item);
        }
    }
}