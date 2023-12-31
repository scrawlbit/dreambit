﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static Rectangle Positive(this Rectangle rectangle)
        {
            if (rectangle.Width < 0)
            {
                rectangle.Width = Math.Abs(rectangle.Width);
                rectangle.X -= rectangle.Width;
            }

            if (rectangle.Height < 0)
            {
                rectangle.Height = Math.Abs(rectangle.Height);
                rectangle.Y -= rectangle.Height;
            }

            return rectangle;
        }

        public static Point LeftTop(this Rectangle rectangle)
        {
            return new Point(rectangle.Left, rectangle.Top);
        }
        public static Point LeftCenter(this Rectangle rectangle)
        {
            return new Point(rectangle.Left, rectangle.Center.Y);
        }
        public static Point LeftBottom(this Rectangle rectangle)
        {
            return new Point(rectangle.Left, rectangle.Bottom);
        }
        public static Point CenterTop(this Rectangle rectangle)
        {
            return new Point(rectangle.Center.X, rectangle.Top);
        }
        public static Point CenterBottom(this Rectangle rectangle)
        {
            return new Point(rectangle.Center.X, rectangle.Bottom);
        }
        public static Point RightTop(this Rectangle rectangle)
        {
            return new Point(rectangle.Right, rectangle.Top);
        }
        public static Point RightCenter(this Rectangle rectangle)
        {
            return new Point(rectangle.Right, rectangle.Center.Y);
        }
        public static Point RightBottom(this Rectangle rectangle)
        {
            return new Point(rectangle.Right, rectangle.Bottom);
        }

        public static Rectangle Union(IEnumerable<Rectangle> rectangles)
        {
            return Union(rectangles.ToArray());
        }
        public static Rectangle Union(params Rectangle[] rectangles)
        {
            Rectangle union = Rectangle.Empty;

            for (int i = 0; i < rectangles.Length; i++)
            {
                if (i == 0)
                    union = rectangles[i];
                else
                    union = Rectangle.Union(union, rectangles[i]);
            }

            return union;
        }

        public static Rectangle Transform(Rectangle rectangle, Matrix matrix)
        {
            var points = new Vector2[]
            {
                rectangle.LeftTop().ToVector2(),
                rectangle.RightTop().ToVector2(),
                rectangle.RightBottom().ToVector2(),
                rectangle.LeftBottom().ToVector2()
            };

            for (int i = 0; i < points.Length; i++)
                points[i] = Vector2.Transform(points[i], matrix);

            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }
    }
}