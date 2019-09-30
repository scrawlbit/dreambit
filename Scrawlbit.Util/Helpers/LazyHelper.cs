using System;

namespace ScrawlBit.Helpers
{
    public static class LazyHelper
    {
        public static Lazy<T> New<T>(Func<T> factory)
        {
            return new Lazy<T>(factory);
        }
    }
}