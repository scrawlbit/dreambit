using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Scrawlbit.MonoGame.Helpers
{
    // Primitive 2D - https://bitbucket.org/C3/2d-xna-primitives/src/95c26ebe054db4409fd7056b20057671b581dc43/Primitives2D.cs?at=default

    public static class SpriteBatchHelper
    {
        #region Private Members

        private static readonly Dictionary<String, List<Vector2>> CircleCache = new Dictionary<string, List<Vector2>>();
        private static Texture2D _pixel;

        #endregion


        #region Private Methods

        private static void CreateThePixel(SpriteBatch spriteBatch)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _pixel.SetData(new[] { Color.White });
        }


        /// <summary>
        /// Draws a list of connecting points
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// /// <param name="position">Where to position the points</param>
        /// <param name="points">The points to connect with lines</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the lines</param>
        private static void DrawPoints(SpriteBatch spriteBatch, Vector2 position, List<Vector2> points, Color color, float thickness)
        {
            if (points.Count < 2)
                return;

            for (int i = 1; i < points.Count; i++)
            {
                DrawLine(spriteBatch, points[i - 1] + position, points[i] + position, color, thickness);
            }
        }


        /// <summary>
        /// Creates a list of vectors that represents a circle
        /// </summary>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <returns>A list of vectors that, if connected, will create a circle</returns>
        private static List<Vector2> CreateCircle(double radius, int sides)
        {
            // Look for a cached version of this circle
            String circleKey = radius + "x" + sides;
            if (CircleCache.ContainsKey(circleKey))
            {
                return CircleCache[circleKey];
            }

            var vectors = new List<Vector2>();

            const double max = 2.0 * Math.PI;
            double step = max / sides;

            for (double theta = 0.0; theta < max; theta += step)
            {
                vectors.Add(new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta))));
            }

            // then add the first vector again so it's a complete loop
            vectors.Add(new Vector2((float)(radius * Math.Cos(0)), (float)(radius * Math.Sin(0))));

            // Cache this circle so that it can be quickly drawn next time
            CircleCache.Add(circleKey, vectors);

            return vectors;
        }


        /// <summary>
        /// Creates a list of vectors that represents an arc
        /// </summary>
        /// <param name="radius">The radius of the arc</param>
        /// <param name="sides">The number of sides to generate in the circle that this will cut out from</param>
        /// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
        /// <param name="radians">The radians to draw, clockwise from the starting angle</param>
        /// <returns>A list of vectors that, if connected, will create an arc</returns>
        private static List<Vector2> CreateArc(float radius, int sides, float startingAngle, float radians)
        {
            var points = new List<Vector2>();
            points.AddRange(CreateCircle(radius, sides));
            points.RemoveAt(points.Count - 1); // remove the last point because it's a duplicate of the first

            // The circle starts at (radius, 0)
            double curAngle = 0.0;
            double anglePerSide = MathHelper.TwoPi / sides;

            // "Rotate" to the starting point
            while ((curAngle + (anglePerSide / 2.0)) < startingAngle)
            {
                curAngle += anglePerSide;

                // move the first point to the end
                points.Add(points[0]);
                points.RemoveAt(0);
            }

            // Add the first point, just in case we make a full circle
            points.Add(points[0]);

            // Now remove the points at the end of the circle to create the arc
            var sidesInArc = (int)((radians / anglePerSide) + 0.5);
            points.RemoveRange(sidesInArc + 1, points.Count - sidesInArc - 1);

            return points;
        }

        #endregion


        #region FillRectangle

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The color to draw the rectangle in</param>
        public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            if (_pixel == null)
            {
                CreateThePixel(spriteBatch);
            }

            // Simply use the function already there
            spriteBatch.Draw(_pixel, rect, color);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="angle">The angle in radians to draw the rectangle at</param>
        public static void FillRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float angle)
        {
            if (_pixel == null)
            {
                CreateThePixel(spriteBatch);
            }

            spriteBatch.Draw(_pixel, rect, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="angle">The angle in radians to draw the rectangle at</param>
        /// <param name="color">The color to draw the rectangle in</param>
        public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float angle = 0f)
        {
            if (_pixel == null)
            {
                CreateThePixel(spriteBatch);
            }

            // stretch the pixel between the two vectors
            spriteBatch.Draw(_pixel,
                             location,
                             null,
                             color,
                             angle,
                             Vector2.Zero,
                             size,
                             SpriteEffects.None,
                             0);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x">The X coord of the left side</param>
        /// <param name="y">The Y coord of the upper side</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="color">The color to draw the rectangle in</param>
        public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float w, float h, Color color)
        {
            FillRectangle(spriteBatch, new Vector2(x, y), new Vector2(w, h), color);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x">The X coord of the left side</param>
        /// <param name="y">The Y coord of the upper side</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="angle">The angle of the rectangle in radians</param>
        public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float w, float h, Color color, float angle)
        {
            FillRectangle(spriteBatch, new Vector2(x, y), new Vector2(w, h), color, angle);
        }

        #endregion


        #region DrawRectangle

        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="thickness">The thickness of the lines</param>
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color, float thickness = 1f)
        {

            // TODO: Handle rotations
            // TODO: Figure out the pattern for the offsets required and then handle it in the line instead of here

            DrawLine(spriteBatch, new Vector2(rect.X, rect.Y), new Vector2(rect.Right, rect.Y), color, thickness); // top
            DrawLine(spriteBatch, new Vector2(rect.X + 1f, rect.Y), new Vector2(rect.X + 1f, rect.Bottom + thickness), color, thickness); // left
            DrawLine(spriteBatch, new Vector2(rect.X, rect.Bottom), new Vector2(rect.Right, rect.Bottom), color, thickness); // bottom
            DrawLine(spriteBatch, new Vector2(rect.Right + 1f, rect.Y), new Vector2(rect.Right + 1f, rect.Bottom + thickness), color, thickness); // right
        }


        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="color">The color to draw the rectangle in</param>
        public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color)
        {
            DrawRectangle(spriteBatch, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color);
        }


        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="thickness">The thickness of the line</param>
        public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 location, Vector2 size, Color color, float thickness)
        {
            DrawRectangle(spriteBatch, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);
        }

        #endregion


        #region DrawLine

        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x1">The X coord of the first point</param>
        /// <param name="y1">The Y coord of the first point</param>
        /// <param name="x2">The X coord of the second point</param>
        /// <param name="y2">The Y coord of the second point</param>
        /// <param name="color">The color to use</param>
        public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color)
        {
            DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color);
        }


        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x1">The X coord of the first point</param>
        /// <param name="y1">The Y coord of the first point</param>
        /// <param name="x2">The X coord of the second point</param>
        /// <param name="y2">The Y coord of the second point</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the line</param>
        public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness)
        {
            DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
        }


        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the line</param>
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            // calculate the distance between the two vectors
            var distance = Vector2.Distance(point1, point2);

            // calculate the angle between the two vectors
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            DrawLine(spriteBatch, point1, distance, angle, color, thickness);
        }


        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="point">The starting point</param>
        /// <param name="length">The length of the line</param>
        /// <param name="angle">The angle of this line from the starting point</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the line</param>
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            if (_pixel == null)
            {
                CreateThePixel(spriteBatch);
            }

            if (thickness > 1)
            {
                const int fix = 1;
                if (angle > -MathHelper.Pi && angle <= -MathHelper.PiOver2)
                    point += new Vector2(0);
                else if (angle > -MathHelper.PiOver4 && angle <= MathHelper.PiOver4)
                    point += new Vector2(0, -fix);
                if (angle > MathHelper.PiOver4 && angle <= MathHelper.PiOver2)
                    point += new Vector2(fix, -fix);
                else if (angle > MathHelper.PiOver2 && angle <= MathHelper.Pi)
                    point += new Vector2(0, 0);

                var round = (float)Math.Round(thickness / 2);
                if (angle > -MathHelper.Pi && angle <= -MathHelper.PiOver2)
                    point -= new Vector2(round, 0);
                else if (angle > -MathHelper.PiOver4 && angle <= MathHelper.PiOver4)
                    point -= new Vector2(0, round);
                if (angle > MathHelper.PiOver4 && angle <= MathHelper.PiOver2)
                    point += new Vector2(round, 0);
                else if (angle > MathHelper.PiOver2 && angle <= MathHelper.Pi)
                    point += new Vector2(0, round);
            }

            // stretch the pixel between the two vectors
            spriteBatch.Draw(_pixel,
                             point,
                             null,
                             color,
                             angle,
                             Vector2.Zero,
                             new Vector2(length, thickness),
                             SpriteEffects.None,
                             0);
        }

        #endregion


        #region PutPixel

        public static void PutPixel(this SpriteBatch spriteBatch, float x, float y, Color color)
        {
            PutPixel(spriteBatch, new Vector2(x, y), color);
        }


        public static void PutPixel(this SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            if (_pixel == null)
            {
                CreateThePixel(spriteBatch);
            }

            spriteBatch.Draw(_pixel, position, color);
        }

        #endregion


        #region DrawCircle

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The color of the circle</param>
        public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color)
        {
            DrawPoints(spriteBatch, center, CreateCircle(radius, sides), color, 1.0f);
        }


        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="thickness">The thickness of the lines used</param>
        public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color, float thickness)
        {
            DrawPoints(spriteBatch, center, CreateCircle(radius, sides), color, thickness);
        }


        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x">The center X of the circle</param>
        /// <param name="y">The center Y of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The color of the circle</param>
        public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color)
        {
            DrawPoints(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, 1.0f);
        }


        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x">The center X of the circle</param>
        /// <param name="y">The center Y of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="thickness">The thickness of the lines used</param>
        public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color, float thickness)
        {
            DrawPoints(spriteBatch, new Vector2(x, y), CreateCircle(radius, sides), color, thickness);
        }

        #endregion


        #region DrawArc

        /// <summary>
        /// Draw a arc
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="center">The center of the arc</param>
        /// <param name="radius">The radius of the arc</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
        /// <param name="radians">The number of radians to draw, clockwise from the starting angle</param>
        /// <param name="color">The color of the arc</param>
        /// <param name="thickness">The thickness of the arc</param>
        public static void DrawArc(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, float startingAngle, float radians, Color color, float thickness = 1f)
        {
            List<Vector2> arc = CreateArc(radius, sides, startingAngle, radians);
            //List<Vector2> arc = CreateArc2(radius, sides, startingAngle, degrees);
            DrawPoints(spriteBatch, center, arc, color, thickness);
        }

        #endregion


        #region DrawTriangule
        public static void DrawTriangle(this SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color, int thickness = 1)
        {
            spriteBatch.DrawLine(p1, p2, color, thickness);
            spriteBatch.DrawLine(p1, p3, color, thickness);
            spriteBatch.DrawLine(p2, p3, color, thickness);
        }
        public static void DrawTriangle(this SpriteBatch spriteBatch, int x1, int y1, int x2, int y2, int x3, int y3, Color color, int thickness)
        {
            spriteBatch.DrawLine(x1, y1, x2, y2, color, thickness);
            spriteBatch.DrawLine(x1, y1, x3, y3, color, thickness);
            spriteBatch.DrawLine(x2, y2, x3, y3, color, thickness);
        }
        public static void DrawTriangle(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, float x3, float y3, Color color, float thickness)
        {
            spriteBatch.DrawTriangle((int)x1, (int)y1, (int)x2, (int)y2, (int)x3, (int)y3, color, (int)thickness);
        }
        public static void FillTriangle(this SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
        {
            // Order points by swapping them around
            Vector2 tp;
            if (p2.Y > p1.Y) { tp = p1; p1 = p2; p2 = tp; }
            if (p3.Y > p1.Y) { tp = p1; p1 = p3; p3 = tp; }
            if (p3.Y > p2.Y) { tp = p2; p2 = p3; p3 = tp; }

            // Number of steps for each edge
            int steps13 = (int)(p1.Y - p3.Y);
            int steps12 = (int)(p1.Y - p2.Y);
            int steps23 = (int)(p2.Y - p3.Y);

            // Displacement value for interpolation
            float sx13 = (p1.X - p3.X) / steps13;
            float sx12 = (p1.X - p2.X) / steps12;
            float sx23 = (p2.X - p3.X) / steps23;

            // Draw LOWER part of triangle
            float x13 = p1.X;
            float x12 = p1.X;
            float dx;   // Distance (length) of horizontal line to draw
            for (int i = 0; i < steps12; i++)
            {
                // Decrease by step
                x13 -= sx13;
                x12 -= sx12;
                // Calculate new distance between points of the two edges
                dx = x13 - x12;
                // Draw horizontal line (rectangle used)
                if (dx > 0)
                    spriteBatch.FillRectangle((int)x12, (int)p1.Y - i, (int)Math.Abs(dx), 1, color);
                else
                    spriteBatch.FillRectangle((int)x13, (int)p1.Y - i, (int)Math.Abs(dx), 1, color);
            }
            // Draw upper part of triangle
            // x13 should still hold a midpoint value from last loop so we need to 
            // connect this with points of x23 starting at point 2 (p2)
            float x23 = p2.X;
            for (int i = 0; i < steps23; i++)
            {
                // Decrease both by step as we move towards top corner
                x13 -= sx13;
                x23 -= sx23;
                // Length of horizontal line to draw
                dx = x13 - x23;
                if (dx > 0)
                    spriteBatch.FillRectangle((int)x23, (int)p2.Y - i, (int)Math.Abs(dx), 1, color);
                else
                    spriteBatch.FillRectangle((int)x13, (int)p2.Y - i, (int)Math.Abs(dx), 1, color);
            }
        }
        public static void FillTriangle(this SpriteBatch spriteBatch, int x1, int y1, int x2, int y2, int x3, int y3, Color color)
        {
            spriteBatch.FillTriangle(new Vector2(x1, y1), new Vector2(x2, y2), new Vector2(x3, y3), color);
        }
        public static void FillTriangle(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, float x3, float y3, Color color)
        {
            spriteBatch.FillTriangle((int)x1, (int)y1, (int)x2, (int)y2, (int)x3, (int)y3, color);
        }
        #endregion

    }
}
