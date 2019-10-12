namespace DreamBit.Game.Helpers
{
    public static class PropertyHelper
    {
        public static bool SetTo<T>(this T value, ref T variable)
        {
            if (!Equals(variable, value))
            {
                variable = value;
                return true;
            }

            return false;
        }
    }
}