using System;
using System.Runtime.InteropServices;

namespace DreamBit.Extension.Helpers
{
    public static class MarshalHelper
    {
        public static void Release(this IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
                Marshal.Release(pointer);
        }
    }
}