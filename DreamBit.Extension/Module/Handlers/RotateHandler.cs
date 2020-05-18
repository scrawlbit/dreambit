using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Extension.Resources;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using ScrawlBit.MonoGame.Interop.Controls;
using System;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Handlers
{
    internal interface IRotateHandler : IEditorHandler { }
    internal class RotateHandler : EditorHandler, IRotateHandler
    {
        private const float Transparency = .7f;
        private const int HandleCircleSize = 5;
        private const int RadiusDivisor = 2;
        private const int FixedAngle = 5;
        private const int Square = HandleCircleSize * 2;

        private readonly IEditor _editor;
        private bool _fixedAngle;
        private Rectangle _selectionArea;

        public RotateHandler(IEditor editor)
        {
            _editor = editor;

            Cursor = CustomCursors.HandOpen;
        }

        public override bool IsMouseOver(Vector2 position)
        {
            Rectangle selectionArea = GetSelectionArea();
            int sides = CalculateSides(selectionArea);
            float radius = CalculateRadius(sides);
            Rectangle handle = RectangleHandle(radius);

            return handle.Contains(position);
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key.IsControlOrShift())
            {
                _fixedAngle = true;
                FixAngle();
            }
        }
        public override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key.IsControlOrShift())
            {
                _fixedAngle = false;
                FixAngle();
            }
        }

        public override void OnMouseDown(GameMouseButtonEventArgs e)
        {
            if (!e.Handled && e.ChangedButton == MouseButton.Left && _editor.Selection.HasSelection)
            {
                IsHandling = IsMouseOver(e.Position);

                if (IsHandling)
                {
                    _selectionArea = _editor.Selection.Area();
                    Cursor = CustomCursors.HandClose;
                }
            }
        }
        public override void OnMouseMove(GameMouseEventArgs e)
        {
            if (!IsHandling) return;

            Vector2 direction = _editor.Camera.ScreenToWorld(e.Position) - _editor.Selection.Position;
            float angle = Vector2Helper.DirectionToAngle(direction);

            _editor.Selection.Rotation = angle;

            FixAngle();
        }
        public override void OnMouseUp(GameMouseButtonEventArgs e)
        {
            if (IsHandling && e.ChangedButton == MouseButton.Left)
            {
                _editor.Selection.ValidateChanges();

                Cursor = CustomCursors.HandOpen;
                IsHandling = false;
            }
        }

        public override void Draw(IContentDrawer drawer)
        {
            if (!_editor.Selection.HasSelection) return;

            Rectangle selectionArea = GetSelectionArea();
            int sides = CalculateSides(selectionArea);
            float radius = CalculateRadius(sides);
            Vector2 center = _editor.Camera.WorldToScreen(_editor.Selection.Position);
            Rectangle handle = RectangleHandle(radius);

            drawer.DrawCircle(center, radius, sides, Color.White);
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
        private Rectangle RectangleHandle(float radius)
        {
            Vector2 center = _editor.Camera.WorldToScreen(_editor.Selection.Position);
            float x = (float)Math.Cos(_editor.Selection.Rotation - MathHelper.PiOver2) * radius;
            float y = (float)Math.Sin(_editor.Selection.Rotation - MathHelper.PiOver2) * radius;
            Vector2 position = new Vector2(x, y) + center;

            return new Rectangle((int)(position.X - HandleCircleSize), (int)(position.Y - HandleCircleSize), Square, Square);
        }

        private Rectangle GetSelectionArea()
        {
            return IsHandling ? _selectionArea : _editor.Selection.Area();
        }
        private void FixAngle()
        {
            if (IsHandling && _fixedAngle)
                _editor.Selection.Rotation = MathematicHelper.RoundAngle(_editor.Selection.Rotation, FixedAngle);
        }
    }
}
