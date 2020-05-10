using Microsoft.Xna.Framework;

namespace Scrawlbit.MonoGame.Helpers
{
    public static class PointHelper
    {
        public static readonly Point Zero = new Point(0, 0);

        public static bool IsEmpty(this Point point)
        {
            return point == Zero;
        }

        public static Point Transform(Point point, Matrix matrix)
        {
            return Vector2.Transform(point.ToVector2(), matrix).ToPoint();
        }
    }
}