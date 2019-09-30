using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Scrawlbit.Presentation.Helpers;

namespace Scrawlbit.Presentation.DragAndDrop
{
    public class DraggableBehavior : Behavior<FrameworkElement>
    {
        private bool _isMouseClicked;
        private MultipleChildDraggableBehavior _multipleDataBehavior;

        protected override void OnAttached()
        {
            base.OnAttached();

            _multipleDataBehavior = AssociatedObject.FindBehaviorInParent<MultipleChildDraggableBehavior>();

            AssociatedObject.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, (MouseButtonEventHandler)OnMouseButtonDown, true);
            AssociatedObject.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent, (MouseButtonEventHandler)OnMouseButtonUp, true);
            AssociatedObject.AddHandler(UIElement.MouseLeaveEvent, (MouseEventHandler)OnMouseLeave, true);
        }
        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, (MouseButtonEventHandler)OnMouseButtonDown);
            AssociatedObject.RemoveHandler(UIElement.PreviewMouseLeftButtonUpEvent, (MouseButtonEventHandler)OnMouseButtonUp);
            AssociatedObject.RemoveHandler(UIElement.MouseLeaveEvent, (MouseEventHandler)OnMouseLeave);

            base.OnDetaching();
        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseClicked = true;
        }
        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseClicked = false;
        }
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isMouseClicked) return;

            if (AssociatedObject.DataContext != null)
                DragDrop.DoDragDrop(AssociatedObject, GetDataObject(GetData()), DragDropEffects.Move);

            _isMouseClicked = false;
        }

        private object[] GetData()
        {
            var data = new List<object>();
            var multipleData = _multipleDataBehavior?.SelectedChildrenValues?.Cast<object>().ToArray() ?? new object[0];

            if (multipleData.Contains(AssociatedObject.DataContext))
                data.AddRange(multipleData);
            else
                data.Add(AssociatedObject.DataContext);

            return data.ToArray();
        }
        protected virtual DataObject GetDataObject(object[] data)
        {
            return new DataObject(typeof(object[]), data);
        }
    }
}