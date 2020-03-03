namespace Scrawlbit.Helpers
{
    public static class StringHelper
    {
        public static bool HasValue(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
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