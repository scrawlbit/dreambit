using System;

namespace ScrawlBit.Helpers
{
    public static class ArrayHelper
    {
        public static int IndexOf<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value);
        }
    }
}