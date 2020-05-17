using DreamBit.Extension.Management;
using DreamBit.Extension.Resources;
using DreamBit.Game.Drawing;
using DreamBit.General.State;
using Microsoft.Xna.Framework;
using ScrawlBit.MonoGame.Interop.Controls;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Handlers
{
    internal interface IMoveHandler : IEditorHandler { }
    internal class MoveHandler : EditorHandler, IMoveHandler
    {
        private const int AxisSize = 60;
        private const int TrianguleSize = 11;
        private const int HalfTrianguleSize = 5;
        private const int HandlerMoveSize = 20;
        private const float Transparency = .7f;

        private readonly IEditor _editor;
        private Vector2 _initialPosition;
        private Vector2 _initialMousePosition;
        private bool _isMovingHorizontally;
        private bool _isMovingVertically;

        public MoveHandler(IEditor editor)
        {
            _editor = editor;

            Cursor = CustomCursors.HandOpen;
        }

        public override bool IsMouseOver(Vector2 position)
        {
            return IsMouseOverMiddleHandler(position) || IsMouseOverVerticalHandler(position) || IsMouseOverHorizontalHandler(position);
        }

        public override void OnMouseDown(GameMouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _editor.Selection.HasSelection)
            {
                IsHandling = IsMouseOverMiddleHandler(e.Position);

                _isMovingHorizontally = IsHandling || IsMouseOverHorizontalHandler(e.Position);
                _isMovingVertically = IsHandling || IsMouseOverVerticalHandler(e.Position);

                IsHandling = _isMovingHorizontally || _isMovingVertically;

                if (IsHandling)
                {
                    _initialMousePosition = _editor.Camera.ScreenToWorld(e.Position);
                    _initialPosition = _editor.Selection.Position;

                    Cursor = CustomCursors.HandClose;
                }
            }
        }
        public override void OnMouseMove(GameMouseEventArgs e)
        {
            if (!IsHandling) return;

            var change = _editor.Camera.ScreenToWorld(e.Position) - _initialMousePosition;
            var position = _initialPosition;

            if (_isMovingHorizontally)
                position.X += change.X;

            if (_isMovingVertically)
                position.Y += change.Y;

            _editor.Selection.Position = position;
        }
        public override void OnMouseUp(GameMouseButtonEventArgs e)
        {
            if (IsHandling && e.ChangedButton == MouseButton.Left)
            {
                _editor.Selection.ValidateChanges();

                IsHandling = false;
                Cursor = CustomCursors.HandOpen;
            }
        }

        public override void Draw(IContentDrawer drawer)
        {
            if (!_editor.Selection.HasSelection)
                return;

            Vector2 position = _editor.Camera.WorldToScreen(_editor.Selection.Position);
            Vector2 top = position + new Vector2(0, -AxisSize);
            Vector2 right = position + new Vector2(AxisSize, 0);
            Vector2 handler = position - new Vector2(0, HandlerMoveSize);

            drawer.DrawLine(position, top, Color.Red);
            drawer.FillTriangle(
                top + new Vector2(-HalfTrianguleSize, 0),
                top + new Vector2(0, -TrianguleSize),
                top + new Vector2(HalfTrianguleSize, 0), Color.Red);

            drawer.DrawLine(position, right, Color.Lime);
            drawer.FillTriangle(
                right + new Vector2(0, -HalfTrianguleSize),
                right + new Vector2(TrianguleSize, 0),
                right + new Vector2(0, HalfTrianguleSize), Color.Lime);

            drawer.FillRectangle(handler, new Vector2(HandlerMoveSize), Color.Gray * Transparency);
            drawer.DrawRectangle(handler, new Vector2(HandlerMoveSize), Color.Gray);
        }

        private bool IsMouseOverMiddleHandler(Vector2 position)
        {
            var handlePosition = _editor.Camera.WorldToScreen(_editor.Selection.Position) - new Vector2(0, HandlerMoveSize);
            var handle = new Rectangle((int)handlePosition.X, (int)handlePosition.Y, HandlerMoveSize, HandlerMoveSize);

            return handle.Contains(position);
        }
        private bool IsMouseOverVerticalHandler(Vector2 position)
        {
            var handlePosition = _editor.Camera.WorldToScreen(_editor.Selection.Position);
            var handle = new Rectangle((int)handlePosition.X, (int)handlePosition.Y - AxisSize, 0, AxisSize);

            handle.Inflate(5, 0);

            return handle.Contains(position);
        }
        private bool IsMouseOverHorizontalHandler(Vector2 position)
        {
            var handlePosition = _editor.Camera.WorldToScreen(_editor.Selection.Position);
            var handle = new Rectangle((int)handlePosition.X, (int)handlePosition.Y, AxisSize, 0);

            handle.Inflate(0, 5);

            return handle.Contains(position);
        }
    }
}
