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
        
        public static Point LeftTop(this Rectangle rect)
        {
            return new Point(rect.Left, rect.Top);
        }
        public static Point RightTop(this Rectangle rect)
        {
            return new Point(rect.Right, rect.Top);
        }
        public static Point LeftBottom(this Rectangle rect)
        {
            return new Point(rect.Left, rect.Bottom);
        }
        public static Point RightBottom(this Rectangle rect)
        {
            return new Point(rect.Right, rect.Bottom);
        }
    }
}