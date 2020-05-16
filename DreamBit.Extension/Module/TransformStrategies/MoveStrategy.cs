using DreamBit.Extension.Management;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;

namespace DreamBit.Extension.Module.TransformStrategies
{
    internal interface IMoveStrategy : ITransformStrategy { }
    internal class MoveStrategy : IMoveStrategy
    {
        private const int AxisSize = 60;
        private const int TrianguleSize = 11;
        private const int HalfTrianguleSize = 5;
        private const int HandlerMoveSize = 20;
        private const float Transparency = .7f;

        private readonly IEditor _editor;

        public MoveStrategy(IEditor editor)
        {
            _editor = editor;
        }

        public bool IsMouseOverHandler(Vector2 position)
        {
            return IsMouseOverMiddleHandler(position) || IsMouseOverVerticalHandler(position) || IsMouseOverHorizontalHandler(position);
        }

        public void Draw(IContentDrawer drawer)
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
