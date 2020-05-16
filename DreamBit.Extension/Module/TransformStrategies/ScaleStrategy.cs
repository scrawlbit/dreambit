using DreamBit.Extension.Management;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Extension.Module.TransformStrategies
{
    internal interface IScaleStrategy : ITransformStrategy { }
    internal class ScaleStrategy : IScaleStrategy
    {
        private const int HandlerSize = 4;
        private readonly IEditor _editor;

        public ScaleStrategy(IEditor editor)
        {
            _editor = editor;
        }

        public void Draw(IContentDrawer drawer)
        {
            if (!_editor.Selection.HasSelection)
                return;

            Rectangle selectionArea = _editor.Selection.Area();
            Point LeftTop = selectionArea.LeftTop();
            Point LeftCenter = selectionArea.LeftCenter();
            Point LeftBottom = selectionArea.LeftBottom();
            Point CenterTop = selectionArea.CenterTop();
            Point CenterBottom = selectionArea.CenterBottom();
            Point RightTop = selectionArea.RightTop();
            Point RightCenter = selectionArea.RightCenter();
            Point RightBottom = selectionArea.RightBottom();

            DrawHandle(drawer, LeftTop);
            DrawHandle(drawer, LeftCenter);
            DrawHandle(drawer, LeftBottom);
            DrawHandle(drawer, CenterTop);
            DrawHandle(drawer, CenterBottom);
            DrawHandle(drawer, RightTop);
            DrawHandle(drawer, RightCenter);
            DrawHandle(drawer, RightBottom);
        }

        private void DrawHandle(IContentDrawer spriteBatch, Point point)
        {
            var rectangle = CalculateRectangle(point);

            spriteBatch.FillRectangle(rectangle, Color.Gray * 0.7f);
        }
        private Rectangle CalculateRectangle(Point point)
        {
            point = _editor.Camera.WorldToScreen(point);

            var rectangle = new Rectangle(point, Point.Zero);
            rectangle.Inflate(HandlerSize, HandlerSize);

            return rectangle;
        }
    }
}
