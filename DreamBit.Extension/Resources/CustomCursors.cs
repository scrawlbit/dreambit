using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;

namespace DreamBit.Extension.Resources
{
    public static class CustomCursors
    {
        private static Cursor _handOpen;
        private static Cursor _handClose;

        public static Cursor HandOpen => Get("handOpen", ref _handOpen);
        public static Cursor HandClose => Get("handClose", ref _handClose);

        private static Cursor Get(string name, ref Cursor cursor)
        {
            if (cursor == null)
            {
                Uri path = DreamBitPackage.GetResourceUri($"Resources/Cursors/{name}.cur");
                StreamResourceInfo info = Application.GetResourceStream(path);

                cursor = new Cursor(info.Stream);
            }

            return cursor;
        }
    }
}
