using Microsoft.Xna.Framework;

namespace Scrawlbit.MonoGame.Helpers
{
    public static class MathematicHelper
    {
        public static float RoundAngle(float rotation, int fixedDegrees)
        {
            return MathHelper.ToRadians((int)(MathHelper.ToDegrees(rotation) / fixedDegrees) * fixedDegrees);
        }
    }
}