using Scrawlbit.Presentation.DragAndDrop;
using Scrawlbit.Presentation.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TreeViewItem = System.Windows.Controls.MultiSelectTreeViewItem;

namespace DreamBit.Extension.Controls.DragAndDrop
{
    public class TreeViewItemAdorner : DroppableAdorner
    {
        private TreeViewItem _item;
        private DroppableAdorner _parentAdorner;
        private bool _isParentFirstElement;
        private ContentPresenter _header;
        private DropType _dropType;

        public TreeViewItemAdorner(UIElement adornedElement) : base(adornedElement)
        {
            BorderBrush = ApplicationHelper.FindResource<SolidColorBrush>("DroppableAdornerBorderBrush");
            BackgroundBrush = ApplicationHelper.FindResource<SolidColorBrush>("DroppableAdornerBackgroundBrush");
            LineBrush = ApplicationHelper.FindResource<SolidColorBrush>("DroppableAdornerLineBrush");
        }

        public SolidColorBrush BorderBrush { get; set; }
        public SolidColorBrush BackgroundBrush { get; set; }
        public SolidColorBrush LineBrush { get; set; }

        public override void Update(DropType dropType, object[] data, object target)
        {
            if (data != null && data.Contains(target))
                return;

            _parentAdorner?.Remove();
            _dropType = dropType;

            base.Update(dropType, data, target);
        }
        public override void Remove()
        {
            _dropType = 0;
            _parentAdorner?.Remove();
            base.Remove();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Initialize();

            if (_dropType == DropType.Inside)
            {
                DrawBox(drawingContext);
            }
            else if (_dropType == DropType.InsideOnTop)
            {
                DrawBox(drawingContext);
                DrawLine(drawingContext, true);
            }
            else if (_dropType == DropType.Below)
            {
                _parentAdorner?.Update(DropType.Inside, null, null);
                DrawLine(drawingContext, true);
            }
            else if (_dropType == DropType.Above)
            {
                if (_isParentFirstElement)
                {
                    _parentAdorner.Update(DropType.InsideOnTop, null, null);
                }
                else
                {
                    _parentAdorner?.Update(DropType.Inside, null, null);
                    DrawLine(drawingContext, false);
                }
            }
        }

        private void Initialize()
        {
            if (_item != null)
                return;

            _item = AdornedElement.ClosestUntil<TreeViewItem>();
            _header = AdornedElement.FindChild<ContentPresenter>();

            var parentItem = _item.ParentsUntil<TreeViewItem>();

            _parentAdorner = parentItem?.FindBehaviorInside<AdornedTreeViewDroppableBehavior>().Adorner;
            _isParentFirstElement = parentItem?.Items[0] == _item.DataContext;
        }
        private void DrawBox(DrawingContext drawingContext)
        {
            if (_item.IsSelected)
                return;

            var size = new Size(AdornedElement.RenderSize.Width - 2, AdornedElement.RenderSize.Height - 2);
            var rect = new Rect(new Point(1, 1), size);
            var pen = new Pen(BorderBrush, 2);

            drawingContext.DrawRoundedRectangle(BackgroundBrush, pen, rect, 2, 2);
        }
        private void DrawLine(DrawingContext drawingContext, bool onBottom)
        {
            var point = _header.TranslatePoint(new Point(), AdornedElement);

            var rect = new Rect(_header.RenderSize);
            var start = new Point(point.X, onBottom ? rect.Bottom : rect.Top);
            var end = new Point(Math.Max(70, rect.Width), start.Y);
            var pen = new Pen(LineBrush, 2);

            drawingContext.DrawLine(pen, start, end);
            drawingContext.DrawEllipse(LineBrush, pen, start, 2, 2);
        }
    }
}
