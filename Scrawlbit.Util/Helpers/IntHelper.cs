using System;

namespace Scrawlbit.Helpers
{
    public static class IntHelper
    {
        public static bool BetweenMargin(this int number, int numberCompare, int margin)
        {
            return number.Between(numberCompare - margin, numberCompare + margin);
        }

        public static bool Between(this int num, int lower, int upper, bool inclusive = false)
        {
            return inclusive
                ? lower <= num && num <= upper
                : lower < num && num < upper;
        }

        public static int ToInt(this string value)
        {
            return Convert.ToInt32(value);
        }
    }
}