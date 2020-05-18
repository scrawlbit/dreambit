using Scrawlbit.Helpers;
using System.Windows.Input;

namespace DreamBit.Extension.Helpers
{
    public static class KeyHelper
    {
        public static bool IsShift(this Key key)
        {
            return key.In(Key.LeftShift, Key.RightShift);
        }
        public static bool IsControl(this Key key)
        {
            return key.In(Key.LeftCtrl, Key.RightCtrl);
        }
        public static bool IsControlOrShift(this Key key)
        {
            return key.IsControl() || key.IsShift();
        }
    }
}
