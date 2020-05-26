using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Helpers;
using System.Linq;
using System.Windows;
using TreeView = System.Windows.Controls.MultiSelectTreeView;
using TreeViewItem = System.Windows.Controls.MultiSelectTreeViewItem;

namespace Scrawlbit.Presentation.DragAndDrop
{
    public class TreeViewDroppableBehavior : DroppableBehavior
    {
        private TreeView _treeView;
        private TreeViewItem _parentItem;
        private TreeViewItem _item;
        private TreeViewItem[] _dataParentItems;
        private bool _canDropOnEdges;
        private bool _canDropAbove;
        private bool _canDropBelow;
        private bool _canDropInsideOnTop;
        private bool _canDropInside;

        public double EdgeDropMargin { get; set; } = 4;

        protected override void OnAttached()
        {
            base.OnAttached();

            _treeView = AssociatedObject as TreeView;

            if (_treeView == null)
            {
                _item = AssociatedObject.ClosestUntil<TreeViewItem>();
                _parentItem = _item.ParentItem();
                _treeView = _item.ParentTreeView();
                _canDropOnEdges = true;
            }
        }

        protected override void OnDragEnter(object sender, DragEventArgs e)
        {
            base.OnDragEnter(sender, e);

            var dataItems = e.Data.GetData(typeof(TreeViewItem[])) as TreeViewItem[];

            _dataParentItems = dataItems?.Select(i => i.ParentItem()).ToArray();
            _canDropAbove = false;
            _canDropBelow = false;
            _canDropInsideOnTop = false;
            _canDropInside = false;

            if (dataItems == null || dataItems.Length == 0)
                return;

            if (dataItems.Length > 1)
            {
                _canDropInside = true;
                return;
            }

            if (AssociatedObject is TreeView)
            {
                _canDropInside = !Equals(dataItems[0], _treeView.LastItem());
                return;
            }

            var itemsGenerator = _parentItem?.ItemContainerGenerator ?? _treeView.ItemContainerGenerator;
            var index = (_parentItem?.Items ?? _treeView.Items).IndexOf(_item.DataContext);

            var previousItem = itemsGenerator.SafeContainerFromIndex(index - 1);
            var nextItem = itemsGenerator.SafeContainerFromIndex(index + 1);
            var firstChildItem = _item.FindChild<TreeViewItem>();

            _canDropAbove = !Equals(dataItems[0], previousItem);
            _canDropBelow = !Equals(dataItems[0], nextItem);
            _canDropInsideOnTop = !Equals(dataItems[0], firstChildItem);
            _canDropInside = true;
        }
        protected override void OnDragOver(object sender, DragEventArgs e)
        {
            if (_canDropInside)
                base.OnDragOver(sender, e);
            else
                e.Handled = true;
        }
        protected override void OnDrop(object sender, DragEventArgs e)
        {
            if (_canDropInside)
                base.OnDrop(sender, e);
            else
                e.Handled = true;
        }

        protected override DropType GetDropType(DragEventArgs e)
        {
            if (!_canDropOnEdges)
                return base.GetDropType(e);

            var pos = e.GetPosition(AssociatedObject);

            if (_canDropAbove && pos.Y <= EdgeDropMargin)
                return DropType.Above;

            if (pos.Y < AssociatedObject.ActualHeight - EdgeDropMargin)
                return DropType.Inside;

            if (_canDropInsideOnTop && _item.IsExpanded && _item.Items.Count > 0)
                return DropType.InsideOnTop;

            if (_canDropBelow)
                return DropType.Below;

            return DropType.Inside;
        }
        protected override DropEventArgs GetDropEventArgs()
        {
            var args = new TreeViewDropEventArgs
            {
                DropType = DropType,
                Data = Data,
                IsFiles = IsFiles,
                OriginalTarget = AssociatedObject.DataContext
            };

            SetSources(args);

            if (DropType == DropType.Inside)
            {
                SetTargetForDroppingInside(args);

                if (args.HasMultipleData)
                    PreventDroppingOwnChildren(args);
            }
            else if (DropType == DropType.InsideOnTop)
            {
                SetTargetForDroppingInside(args);
            }
            else
            {
                SetTargetForDroppingOnEdges(args);
            }

            if (args.Data.Length == 1)
                ValidateIndexWhenReordering(args);

            return args;
        }

        private void SetSources(TreeViewDropEventArgs args)
        {
            for (var i = 0; i < _dataParentItems?.Length; i++)
            {
                var from = _dataParentItems?[i]?.ItemsSource ?? _treeView.ItemsSource;
                var fromIndex = (_dataParentItems?[i]?.Items ?? _treeView.Items).IndexOf(Data[i]);

                args.Sources.Add(Data[i], new TreeViewDropEventArgs.SourceData
                {
                    Data = Data[i],
                    From = from,
                    FromIndex = fromIndex
                });
            }
        }
        private void SetTargetForDroppingInside(TreeViewDropEventArgs args)
        {
            var items = _item?.Items ?? _treeView.Items;

            args.Target = _item?.DataContext ?? _treeView.DataContext;
            args.To = _item?.ItemsSource ?? _treeView.ItemsSource;
            args.ToIndex = DropType == DropType.InsideOnTop ? 0 : items.Count;
        }
        private void SetTargetForDroppingOnEdges(TreeViewDropEventArgs args)
        {
            var items = _parentItem?.Items ?? _treeView.Items;

            args.Target = _parentItem?.DataContext ?? _treeView.DataContext;
            args.To = _parentItem?.ItemsSource ?? _treeView.ItemsSource;
            args.ToIndex = items.IndexOf(args.OriginalTarget);

            if (DropType == DropType.Below)
                args.ToIndex++;
        }
        private void PreventDroppingOwnChildren(TreeViewDropEventArgs args)
        {
            foreach (var data in args.Data.ToArray())
            {
                if (Equals(args.Sources[data].From, args.To))
                {
                    args.Data = args.Data.Except(data).ToArray();
                    args.Sources.Remove(data);
                }
            }
        }
        private void ValidateIndexWhenReordering(TreeViewDropEventArgs args)
        {
            if (Equals(args.SingleSource.From, args.To) && args.SingleSource.FromIndex < args.ToIndex)
                args.ToIndex--;
        }
    }
}