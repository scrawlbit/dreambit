using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using ScrawlBit.Collections;
using ScrawlBit.Presentation.Helpers;

namespace ScrawlBit.Presentation.Behaviors
{
    public class ListBoxDragAndDropBehavior : Behavior<ListBox>
    {
        private ListBoxItem _dragged;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.DragEnter += OnDragEnter;
            AssociatedObject.Drop += OnDrop;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.AllowDrop = false;
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.DragEnter -= OnDragEnter;
            AssociatedObject.Drop -= OnDrop;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_dragged != null)
                return;

            var element = AssociatedObject.InputHitTest(e.GetPosition(AssociatedObject)) as UIElement;

            while (element != null)
            {
                var listBoxItem = element as ListBoxItem;
                if (listBoxItem != null)
                {
                    _dragged = listBoxItem;
                    break;
                }

                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragged == null)
                return;

            if (e.LeftButton == MouseButtonState.Released)
            {
                _dragged = null;
                return;
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            var obj = new DataObject(DataFormats.Text, _dragged.ToString());
            DragDrop.DoDragDrop(_dragged, obj, DragDropEffects.All);
        }
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (_dragged == null || e.Data.GetDataPresent(DataFormats.Text, true) == false)
                e.Effects = DragDropEffects.None;
            else
                e.Effects = DragDropEffects.All;
        }
        private void OnDrop(object sender, DragEventArgs e)
        {
            var collection = (IObservableCollection)AssociatedObject.ItemsSource;
            var targetItem = ControlHelper.TryFindFromPoint<ListBoxItem>((UIElement)sender, e.GetPosition(AssociatedObject));

            if (targetItem != null && !ReferenceEquals(_dragged, targetItem.DataContext))
            {
                var itemIndex = collection.IndexOf(_dragged.Content);
                var targetIndex = collection.IndexOf(targetItem.DataContext);

                collection.Move(itemIndex, targetIndex);
            }
        }
    }
}