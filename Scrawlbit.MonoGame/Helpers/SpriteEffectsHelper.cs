using Microsoft.Xna.Framework.Graphics;

namespace Scrawlbit.MonoGame.Helpers
{
    public static class SpriteEffectsHelper
    {
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