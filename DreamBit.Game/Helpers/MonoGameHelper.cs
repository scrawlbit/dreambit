using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Helpers
{
    public static class MonoGameHelper
    {
        public static float PositiveAngle(this float angle)
        {
            angle = MathHelper.WrapAngle(angle);

            if (angle < 0)
                angle = MathHelper.TwoPi + angle;

            return angle;
        }

        public static bool EqualTo(this float value, float other, float tolerance = 0)
        {
            return Math.Abs(value - other) <= tolerance;
        }

        public static float MinimumScale(this float scale, float minimum = 0.1f)
        {
            return MathHelper.Max(scale, minimum);
        }
        public static Vector2 MinimumScale(this Vector2 scale, float minimum = 0.1f)
        {
            return Vector2.Max(scale, new Vector2(minimum));
        }

        public static bool SetTo(this float value, ref float variable)
        {
            return SetTo(value, ref variable, 0);
        }
        public static bool SetTo(this float value, ref float variable, float tolerance)
        {
            if (!value.EqualTo(variable, tolerance))
            {
                variable = value;
                return true;
            }

            return false;
        }

        public static SpriteEffects GetEffects(bool flipHorizontally, bool flipVertically)
        {
            var effects = SpriteEffects.None;

            if (flipHorizontally)
                effects |= SpriteEffects.FlipHorizontally;

            if (flipVertically)
                effects |= SpriteEffects.FlipVertically;

            return effects;
        }
    }
}