using System.Collections;
using System.Windows;
using System.Windows.Interactivity;
using ScrawlBit.Presentation.Dependency;

namespace ScrawlBit.Presentation.DragAndDrop
{
    public class MultipleChildDraggableBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty SelectedChildrenValuesProperty;

        static MultipleChildDraggableBehavior()
        {
            var dependency = new DependencyRegistry<MultipleChildDraggableBehavior>();

            SelectedChildrenValuesProperty = dependency.Property(b => b.SelectedChildrenValues);
        }

        public IEnumerable SelectedChildrenValues
        {
            get { return (IEnumerable)GetValue(SelectedChildrenValuesProperty); }
            set { SetValue(SelectedChildrenValuesProperty, value); }
        }
    }
}