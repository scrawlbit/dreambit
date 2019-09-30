using System.Linq;

namespace Scrawlbit.Helpers
{
    public static class ObjectHelper
    {
        public static bool EqualsAny<T>(this T value, params T[] values)
        {
            return values.Any(v => Equals(v, value));
        }
    }
}