using DreamBit.Extension.Management;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using System;

namespace DreamBit.Extension.Module.TransformStrategies
{
    internal interface IRotateStrategy : ITransformStrategy { }
    internal class RotateStrategy : IRotateStrategy
    {
        private const float Transparency = .7f;
        private const float RotationAngle = 1;
        private const int HandleCircleSize = 5;
        private const int RadiusDivisor = 2;
        private const int FixedAngle = 5;
        private const int Square = HandleCircleSize * 2;
        private readonly IEditor _editor;

        public RotateStrategy(IEditor editor)
        {
            _editor = editor;
        }

        public void Draw(IContentDrawer drawer)
        {
            if (!_editor.Selection.HasSelection) return;

            Rectangle selectionArea = _editor.Selection.Area();
            int sides = CalculateSides(selectionArea);
            float radius = CalculateRadius(sides);
            Vector2 center = _editor.Camera.WorldToScreen(_editor.Selection.Position);
            Rectangle handle = RectangleHandle(HandlePosition(radius));

            drawer.DrawCircle(center, radius, sides, Color.Yellow);
            drawer.FillRectangle(handle, Color.Gray * Transparency);
        }

        private static int CalculateSides(Rectangle selectionArea)
        {
            return MathHelper.Max(selectionArea.Width, selectionArea.Height);
        }
        private float CalculateRadius(int sides)
        {
            return sides / RadiusDivisor * _editor.Camera.Zoom;
        }
        private Vector2 HandlePosition(float radius)
        {
            Vector2 center = _editor.Camera.WorldToScreen(_editor.Selection.Position);
            float x = (float)Math.Cos(_editor.Selection.Rotation - MathHelper.PiOver2) * radius;
            float y = (float)Math.Sin(_editor.Selection.Rotation - MathHelper.PiOver2) * radius;

            return new Vector2(x, y) + center;
        }
        private static Rectangle RectangleHandle(Vector2 handlePosition)
        {
            return new Rectangle((int)(handlePosition.X - HandleCircleSize), (int)(handlePosition.Y - HandleCircleSize), Square, Square);
        }
    }
}
