using System;
using System.Collections.Generic;
using System.Linq;

namespace ScrawlBit.Helpers
{
    public static class StringHelper
    {
        public static bool HasValue(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        public static string GetNewName<T>(this IEnumerable<T> source, string name, Func<T, string> predicate) where T : class
        {
            source = source.ToList();

            var finalName = name;
            var i = 0;

            while (source.Any(l => predicate(l) == finalName))
                finalName = string.Format("{0} {1}", name, ++i);

            return finalName;
        }

        public static string Remove(this string source, string part)
        {
            return source.Replace(part, string.Empty);
        }

        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}