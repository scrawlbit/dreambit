using System;

namespace Scrawlbit.Helpers
{
    public static class ArrayHelper
    {
        public static int IndexOf<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value);
        }
    }
}