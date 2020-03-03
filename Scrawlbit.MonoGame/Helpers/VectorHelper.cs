using System;
using Microsoft.Xna.Framework;

namespace Scrawlbit.MonoGame.Helpers
{
    public static class VectorHelper
    {
        public static bool IsEmpty(this Vector2 value)
        {
            return value == Vector2.Zero;
        }

        public static float DirectionToAngle(Vector2 direction)
        {
            return (float)Math.Atan2(direction.Y, direction.X) + MathHelper.PiOver2;
        }

        public static Vector2 Transform(float x, float y, Matrix matrix)
        {
            return Vector2.Transform(new Vector2(x, y), matrix);
        }

        public static Point RoundToPoint(this Vector2 vector)
        {
            var x = Math.Round(vector.X, 0);
            var y = Math.Round(vector.Y, 0);

            return new Point((int)x, (int)y);
        }

        public static Vector2 MinimumScale(this Vector2 scale, float minimum = 0.1f)
        {
            return Vector2.Max(scale, new Vector2(minimum));
        }
    }
}
