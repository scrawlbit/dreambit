using Microsoft.Xna.Framework;

namespace Scrawlbit.MonoGame.Helpers
{
    public static class RectangleHelper
    {
        public static Rectangle Add(this Rectangle rectangle, Vector2 position)
        {
            return new Rectangle(
                (int)(rectangle.X + position.X),
                (int)(rectangle.Y + position.Y),
                rectangle.Width,
                rectangle.Height
            );
        }

        public static Rectangle ToRectangle(this Vector2 size)
        {
            return Create(size, Vector2.Zero);
        }

        public static Rectangle CalculateBoundingRectangle(this Rectangle rectangle, Matrix transform)
        {
            // Obtem todos os quatro cantos do espaço local
            var leftTop = new Vector2(rectangle.Left, rectangle.Top);
            var rightTop = new Vector2(rectangle.Right, rectangle.Top);
            var leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
            var rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

            // Transforma de todos os quatro cantos para o espaço de trabalho
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            // Encontra as extensões mínima e máxima do retângulo no espaço do mundo
            var min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
            var max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        public static Vector2 Position(this Rectangle rectangle)
        {
            return new Vector2(rectangle.X, rectangle.Y);
        }

        public static Rectangle Create(Vector2 size, Vector2 origin)
        {
            return new Rectangle((-origin).ToPoint(), size.ToPoint());
        }

        public static bool HasSize(this Rectangle rectangle)
        {
            return rectangle.Size != Point.Zero;
        }
    }
}