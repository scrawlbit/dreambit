using System.Globalization;

namespace Scrawlbit.Helpers
{
    public static class FloatHelper
    {
        public static float ToFloat(this string text)
        {
            return float.Parse(text);
        }
        public static float ToFloat(this string text, CultureInfo culture)
        {
            return float.Parse(text, culture);
        }
    }
}