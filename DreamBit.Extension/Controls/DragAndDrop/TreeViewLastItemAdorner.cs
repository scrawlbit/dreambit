using Scrawlbit.Presentation.DragAndDrop;
using Scrawlbit.Presentation.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DreamBit.Extension.Controls.DragAndDrop
{
    public class TreeViewLastItemAdorner : DroppableAdorner
    {
        private readonly ContentPresenter _header;

        public TreeViewLastItemAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _header = adornedElement.FindChild<ContentPresenter>();

            LineBrush = ApplicationHelper.FindResource<SolidColorBrush>("DroppableAdornerLineBrush");
        }

        public SolidColorBrush LineBrush { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            DrawLine(drawingContext);
        }

        private void DrawLine(DrawingContext drawingContext)
        {
            var point = _header.TranslatePoint(new Point(), this);

            var rect = new Rect(AdornedElement.RenderSize);
            var start = new Point(point.X, rect.Bottom);
            var end = new Point(Math.Max(70, rect.Width), start.Y);
            var pen = new Pen(LineBrush, 2);

            drawingContext.DrawLine(pen, start, end);
            drawingContext.DrawEllipse(LineBrush, pen, start, 2, 2);
        }
    }
}
