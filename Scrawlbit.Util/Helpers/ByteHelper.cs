using System;

namespace Scrawlbit.Util.Helpers
{
    public static class ByteHelper
    {
        public static byte ToByte(this string value)
        {
            return Convert.ToByte(value);
        }
    }
}
