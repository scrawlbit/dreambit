using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Presentation.Helpers;
using ScrawlBit.MonoGame.Interop.Controls;
using System;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Handlers
{
    internal interface IScaleHandler : IEditorHandler { }
    internal class ScaleHandler : EditorHandler, IScaleHandler
    {
        private const int HandlerSize = 4;
        private readonly Vector2 _minScale = new Vector2(0.05f);
        private readonly IEditor _editor;
        private Vector2 _initialMousePosition;
        private Vector2 _initialScale;
        private Vector2 _initialSize;
        private bool _isScalingRight;
        private bool _isScalingBottom;
        private bool _isScalingLeft;
        private bool _isScalingTop;

        public ScaleHandler(IEditor editor)
        {
            _editor = editor;
        }

        public override bool IsMouseOver(Vector2 position)
        {
            Rectangle selectionArea = _editor.Selection.Area();
            HandlerOverInfo info = GetMouseHandlerInfo(selectionArea, position);

            if (info.LeftTop || info.RightBottom)
            {
                Cursor = Cursors.SizeNWSE;
                return true;
            }

            if (info.RightTop || info.LeftBottom)
            {
                Cursor = Cursors.SizeNESW;
                return true;
            }

            return false;
        }

        public override void OnMouseDown(GameMouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _editor.Selection.HasSelection)
            {
                Rectangle selectionArea = _editor.Selection.Area();
                HandlerOverInfo info = GetMouseHandlerInfo(selectionArea, e.Position);

                _isScalingRight = info.RightBottom || info.RightTop;
                _isScalingBottom = info.RightBottom || info.LeftBottom;
                _isScalingLeft = info.LeftTop || info.LeftBottom;
                _isScalingTop = info.LeftTop || info.RightTop;

                IsHandling = _isScalingRight || _isScalingBottom || _isScalingLeft || _isScalingTop;

                if (IsHandling)
                {
                    _initialMousePosition = _editor.Camera.ScreenToWorld(e.Position);
                    _initialScale = _editor.Selection.Scale;
                    _initialSize = selectionArea.Size.ToVector2();
                }
            }
        }
        public override void OnMouseMove(GameMouseEventArgs e)
        {
            if (!IsHandling)
                return;

            Vector2 direction = new Vector2();

            if (_isScalingRight) direction.X = 1;
            if (_isScalingLeft) direction.X = -1;
            if (_isScalingTop) direction.Y = -1;
            if (_isScalingBottom) direction.Y = 1;

            Vector2 mousePosition = _editor.Camera.ScreenToWorld(e.Position);
            Vector2 size = _initialSize / _initialScale;
            Vector2 diff = (mousePosition - _initialMousePosition) * direction / size;

            if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                diff.Y = diff.X;
            else
                diff.X = diff.Y;

            _editor.Selection.Scale = Vector2.Max(_initialScale + diff * 2, _minScale);
        }
        public override void OnMouseUp(GameMouseButtonEventArgs e)
        {
            if (IsHandling && e.ChangedButton == MouseButton.Left)
            {
                _editor.Selection.ValidateChanges();

                IsHandling = false;
                IsMouseOver(e.Position);
            }
        }

        public override void Draw(IContentDrawer drawer)
        {
            if (!_editor.Selection.HasSelection)
                return;

            Rectangle area = _editor.Selection.Area();

            DrawHandle(drawer, area.LeftTop());
            DrawHandle(drawer, area.LeftBottom());
            DrawHandle(drawer, area.RightTop());
            DrawHandle(drawer, area.RightBottom());
        }

        private HandlerOverInfo GetMouseHandlerInfo(Rectangle selectionArea, Vector2 position)
        {
            HandlerOverInfo info = new HandlerOverInfo();

            bool IsMouseOver(Point point) => CalculateRectangle(point).Contains(position);

            info.LeftTop = IsMouseOver(selectionArea.LeftTop());
            info.LeftBottom = IsMouseOver(selectionArea.LeftBottom());
            info.RightTop = IsMouseOver(selectionArea.RightTop());
            info.RightBottom = IsMouseOver(selectionArea.RightBottom());

            return info;
        }
        private Rectangle CalculateRectangle(Point point)
        {
            point = _editor.Camera.WorldToScreen(point);

            var rectangle = new Rectangle(point, Point.Zero);
            rectangle.Inflate(HandlerSize, HandlerSize);

            return rectangle;
        }
        private void DrawHandle(IContentDrawer spriteBatch, Point point)
        {
            var rectangle = CalculateRectangle(point);

            spriteBatch.FillRectangle(rectangle, Color.Gray * 0.7f);
        }

        #region HandlerOverInfo

        private struct HandlerOverInfo
        {
            public bool LeftTop { get; set; }
            public bool LeftBottom { get; set; }
            public bool RightTop { get; set; }
            public bool RightBottom { get; set; }
        }

        #endregion
    }
}
