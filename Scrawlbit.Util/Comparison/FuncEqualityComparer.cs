using System;
using System.Collections.Generic;

namespace ScrawlBit.Comparison
{
    internal class FuncEqualityComparer<T, TComparer> : IEqualityComparer<T>
    {
        private readonly Func<T, TComparer> _comparer;

        public FuncEqualityComparer(Func<T, TComparer> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return Equals(_comparer(x), _comparer(y));
        }
        public int GetHashCode(T obj)
        {
            return _comparer(obj).GetHashCode();
        }
    }
}