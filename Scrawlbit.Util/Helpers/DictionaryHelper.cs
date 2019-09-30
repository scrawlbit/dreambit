using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrawlBit.Helpers
{
    public static class DictionaryHelper
    {
        public static IEnumerable<TKey> Keys<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            return dictionary.Where(predicate).Select(d => d.Key);
        }

        public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> source, Action<TKey, TValue> action)
        {
            foreach (var item in source)
                action(item.Key, item.Value);
        }
    }
}