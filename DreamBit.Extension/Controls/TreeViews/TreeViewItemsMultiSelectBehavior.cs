using Microsoft.Xaml.Behaviors;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace DreamBit.Extension.Controls.TreeViews
{
    internal class TreeViewItemsMultiSelectBehavior : Behavior<ItemsControl>
    {
        private readonly TreeViewMultiSelectBehavior _treeViewBehavior;
        private ItemContainerGenerator _generator;
        private readonly IList<Action> _pendingActions;
        private TreeViewItem _item;
        private object _value;
        private NotifyCollectionChangedAction _lastAction;

        public TreeViewItemsMultiSelectBehavior(TreeViewMultiSelectBehavior treeViewBehavior)
        {
            _treeViewBehavior = treeViewBehavior;
            _pendingActions = new List<Action>();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Unloaded += OnUnloaded;

            _generator = AssociatedObject.ItemContainerGenerator;
            _generator.StatusChanged += OnStatusChanged;
            _generator.ItemsChanged += OnItemsChanged;

            if (_generator.Items.Any())
                _pendingActions.Add(OnReset);

            _item = AssociatedObject as TreeViewItem;
            if (_item != null)
            {
                _value = _item.DataContext;

                _item.Selected += OnSelectionChanged;
                _item.Unselected += OnSelectionChanged;
            }
        }
        protected override void OnDetaching()
        {
            AssociatedObject.Unloaded -= OnUnloaded;

            _generator.StatusChanged -= OnStatusChanged;
            _generator.ItemsChanged -= OnItemsChanged;

            if (_item != null)
            {
                _item.Selected -= OnSelectionChanged;
                _item.Unselected -= OnSelectionChanged;
            }

            base.OnDetaching();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_item != null)
                OnSelectionChanged(null, null);

            AssociatedObject.Loaded -= OnLoaded;
        }
        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_item == null) return;
            if (_lastAction != NotifyCollectionChangedAction.Reset) return;
            if (TreeViewMultiSelectBehavior.GetIsMoving(_item)) return;

            _treeViewBehavior.ExecuteWithPriority(() =>
            {
                _item.DataContext = _value;
                _treeViewBehavior.Remove(_item);
                _item.DataContext = BindingOperations.DisconnectedSource;
            });

            Detach();
        }
        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            _treeViewBehavior.ExecuteWithPriority(() =>
            {
                if (_item.IsSelected || _treeViewBehavior.SelectedValues.Contains(_item.DataContext))
                    _treeViewBehavior.Add(_item, _value);
                else
                    _treeViewBehavior.Remove(_item);
            });
        }

        private void OnStatusChanged(object sender, EventArgs e)
        {
            if (_generator.Status == GeneratorStatus.ContainersGenerated)
            {
                _pendingActions.ForEach(a => a());
                _pendingActions.Clear();
            }
        }
        private void OnItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            _lastAction = e.Action;

            if (e.Action == NotifyCollectionChangedAction.Reset)
                _pendingActions.Add(OnReset);

            if (e.Action == NotifyCollectionChangedAction.Add)
                _pendingActions.Add(() => OnAdd(e.Position));
        }

        private void OnReset()
        {
            foreach (var data in _generator.Items)
            {
                AddBehavior((TreeViewItem)_generator.ContainerFromItem(data));
            }
        }
        private void OnAdd(GeneratorPosition position)
        {
            var index = position.Index + position.Offset;
            var item = (TreeViewItem)_generator.ContainerFromIndex(index);

            AddBehavior(item);
        }

        private void AddBehavior(TreeViewItem item)
        {
            if (!item.HasBehavior<TreeViewItemsMultiSelectBehavior>())
                item.AddBehavior(new TreeViewItemsMultiSelectBehavior(_treeViewBehavior));
        }
    }
}
