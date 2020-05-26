using System.Collections;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using Scrawlbit.Presentation.Dependency;

namespace Scrawlbit.Presentation.DragAndDrop
{
    public class MultipleDataDraggableBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty<MultipleDataDraggableBehavior, IEnumerable> ItemsProperty;

        static MultipleDataDraggableBehavior()
        {
            var dependency = new DependencyRegistry<MultipleDataDraggableBehavior>();

            ItemsProperty = dependency.Property(b => b.Items);
        }

        public IEnumerable Items
        {
            get => ItemsProperty.Get(this);
            set => ItemsProperty.Set(this, value);
        }
    }
}