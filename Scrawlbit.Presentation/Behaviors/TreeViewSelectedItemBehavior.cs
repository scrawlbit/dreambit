using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using Scrawlbit.Presentation.Dependency;

namespace Scrawlbit.Presentation.Behaviors
{
    public class TreeViewSelectedItemBehavior : Behavior<TreeView>
    {
        private static readonly DependencyProperty SelectedItemProperty;

        static TreeViewSelectedItemBehavior()
        {
            var dependency = new DependencyRegistry<TreeViewSelectedItemBehavior>();

            SelectedItemProperty = dependency.Property(t => t.SelectedItem);
        }
        
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;

            base.OnDetaching();
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e?.NewValue;
        }
    }
}