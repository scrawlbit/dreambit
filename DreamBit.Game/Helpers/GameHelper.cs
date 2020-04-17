using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace DreamBit.Game.Helpers
{
    public static class GameHelper
    {
        public static float EnsurePrecision(this float value)
        {
            return (float)Math.Round(value, 3);
        }
        public static Vector2 EnsurePrecision(this Vector2 value)
        {
            return new Vector2(value.X.EnsurePrecision(), value.Y.EnsurePrecision());
        }

        public static string Text(this float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static string Text(this Vector2 value)
        {
            string x = value.X.Text();
            string y = value.Y.Text();

            return $"{x};{y}";
        }
    }
}
