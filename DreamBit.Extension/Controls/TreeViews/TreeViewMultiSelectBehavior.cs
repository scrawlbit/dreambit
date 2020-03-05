using Microsoft.Xaml.Behaviors;
using Scrawlbit.Collections;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Dependency;
using Scrawlbit.Presentation.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace DreamBit.Extension.Controls.TreeViews
{
    public class TreeViewMultiSelectBehavior : Behavior<TreeView>
    {
        private static readonly PropertyInfo IsSelectionChangeActiveProperty;
        public static readonly DependencyProperty SelectedValueProperty;
        public static readonly DependencyProperty SelectedValuesProperty;

        private Point? _initialPosition;
        private bool _priorityRequested;

        static TreeViewMultiSelectBehavior()
        {
            IsSelectionChangeActiveProperty = typeof(TreeView).GetProperty("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);

            var dependency = new DependencyRegistry<TreeViewMultiSelectBehavior>();

            SelectedValueProperty = dependency.Property(b => b.SelectedValue, b => b.OnSelectedValueChanged());
            SelectedValuesProperty = dependency.Property(b => b.SelectedValues, (b, e) => b.OnSelectedValuesChanged(e));
            IsMovingItemProperty = dependency.AttachedProperty(GetIsMoving);
        }
        public TreeViewMultiSelectBehavior()
        {
            SelectedItems = new HashSet<TreeViewItem>();
        }

        internal HashSet<TreeViewItem> SelectedItems { get; }
        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }
        public IObservableCollection SelectedValues
        {
            get { return (IObservableCollection)GetValue(SelectedValuesProperty); }
            set { SetValue(SelectedValuesProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            IsSelectionChangeActiveProperty.SetValue(AssociatedObject, true);

            AssociatedObject.Initialized += OnInitialized;
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseleftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnMouseleftButtonUp;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseleftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseleftButtonUp;

            if (SelectedValues != null)
                SelectedValues.CollectionChanged -= OnSelectedValuesCollectionChanged;

            base.OnDetaching();
        }

        private void OnSelectedValueChanged()
        {
            ExecuteWithPriority(() =>
            {
                var item = AssociatedObject.FindTreeViewItem(SelectedValue);

                Select(item, SelectedValue);
            });
        }
        private void OnSelectedValuesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
                throw new NotImplementedException();

            SelectedValues.CollectionChanged += OnSelectedValuesCollectionChanged;
        }
        private void OnSelectedValuesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ExecuteWithPriority(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    SelectedItems.ToArray().ForEach(Remove);

                e.OldItems?.ForEach(o => Remove(AssociatedObject.FindTreeViewItem(o)));
                e.NewItems?.ForEach(o => Add(AssociatedObject.FindTreeViewItem(o), o));
            });
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            AssociatedObject.AddBehavior(new TreeViewItemsMultiSelectBehavior(this));
            AssociatedObject.Initialized -= OnInitialized;
        }
        private void OnMouseleftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _initialPosition = null;

            if (GetClickedExpander(e) != null)
                return;

            _initialPosition = e.GetPosition(AssociatedObject);
            e.Handled = true;
        }
        private void OnMouseleftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var finalPosition = e.GetPosition(AssociatedObject);
            if (finalPosition != _initialPosition)
                return;

            var item = GetClickedTreeViewItem(e);
            if (item == null)
                return;

            var keepSelectedItems = KeyboardHelper.IsCtrlPressed();

            ExecuteWithPriority(() =>
            {
                if (!keepSelectedItems)
                {
                    Select(item, item.DataContext);
                }
                else if (!item.IsSelected)
                {
                    Add(item, item.DataContext);
                }
                else if (SelectedItems.Count > 1)
                {
                    Remove(item);
                }
            });

            e.Handled = true;
        }

        internal void Select(TreeViewItem item, object value)
        {
            SelectedItems.ForEach(i => i.IsSelected = Equals(i, item));
            SelectedItems.RemoveAll(i => !Equals(i, item));
            SelectedValues.RemoveAll(v => !Equals(v, value));

            if (item != null && !SelectedItems.Contains(item))
                SelectedItems.Add(item);

            if (value != null && !SelectedValues.Contains(value))
                SelectedValues.Add(value);

            SelectedValue = value;

            if (item != null)
            {
                item.IsSelected = true;
                item.Focus();
            }
        }
        internal void Add(TreeViewItem item, object value)
        {
            if (item != null && !SelectedItems.Contains(item))
                SelectedItems.Add(item);

            if (!SelectedValues.Contains(value))
                SelectedValues.Add(value);

            if (SelectedValue == null)
                SelectedValue = value;

            if (item != null)
                item.IsSelected = true;

            if (SelectedValue == value)
                item?.Focus();
        }
        internal void Remove(TreeViewItem item)
        {
            SelectedItems.Remove(item);
            SelectedValues.Remove(item.DataContext);

            if (SelectedValue == item.DataContext)
            {
                var newItem = SelectedItems.FirstOrDefault(i => i.DataContext != BindingOperations.DisconnectedSource);

                if (newItem != null)
                {
                    SelectedValue = newItem.DataContext;
                    newItem.IsSelected = true;
                    newItem.Focus();
                }
                else
                {
                    newItem = AssociatedObject.FindChild<TreeViewItem>();
                    Select(newItem, newItem?.DataContext);
                }
            }

            item.IsSelected = false;
        }

        private static ButtonBase GetClickedExpander(RoutedEventArgs e)
        {
            var element = (FrameworkElement)e.OriginalSource;
            return element as ButtonBase ?? element.ParentsUntil<ButtonBase>(d => d is TreeViewItem);
        }
        private static TreeViewItem GetClickedTreeViewItem(RoutedEventArgs e)
        {
            var element = (FrameworkElement)e.OriginalSource;
            return element as TreeViewItem ?? element.ParentsUntil<TreeViewItem>(d => d is TreeView);
        }

        internal void ExecuteWithPriority(Action action)
        {
            if (_priorityRequested)
                return;

            _priorityRequested = true;
            action();
            _priorityRequested = false;
        }

        #region Static Properties

        public static readonly DependencyProperty IsMovingItemProperty;

        public static bool GetIsMoving(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsMovingItemProperty);
        }
        public static void SetIsMoving(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(IsMovingItemProperty, value);
        }

        #endregion
    }
}
