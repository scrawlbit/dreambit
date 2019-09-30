using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrawlBit.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<T> Enumerable<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static T Parse<T>(string value, bool ignoreCase = false)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }
    }
}