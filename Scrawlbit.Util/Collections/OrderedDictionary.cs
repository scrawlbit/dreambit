using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ScrawlBit.Collections
{
    public interface IOrderedDictionary<TKey, TValue>
    {
        int Count { get; }
        TValue this[TKey key] { get; set; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }

        bool ContainsKey(TKey key);
        bool Remove(TKey key);
        void Add(TKey key, TValue value);
    }

    public class OrderedDictionary<TKey, TValue> : OrderedDictionary, IOrderedDictionary<TKey, TValue>
    {
        public TValue this[TKey key]
        {
            get => (TValue)this[(object)key];
            set => this[(object)key] = value;
        }
        public new IEnumerable<TKey> Keys => base.Keys.Cast<TKey>();
        public new IEnumerable<TValue> Values => base.Values.Cast<TValue>();

        public bool ContainsKey(TKey key)
        {
            return Keys.Contains(key);
        }
        public bool Remove(TKey key)
        {
            if (Keys.Contains(key))
            {
                base.Remove(key);
                return true;
            }

            return false;
        }
        public void Add(TKey key, TValue value)
        {
            base.Add(key, value);
        }
    }
}