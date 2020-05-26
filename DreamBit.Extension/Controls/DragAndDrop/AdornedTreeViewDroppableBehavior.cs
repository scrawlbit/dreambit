using Scrawlbit.Presentation.DragAndDrop;
using Scrawlbit.Presentation.Helpers;
using System.Windows;
using TreeView = System.Windows.Controls.MultiSelectTreeView;

namespace DreamBit.Extension.Controls.DragAndDrop
{
    public class AdornedTreeViewDroppableBehavior : TreeViewDroppableBehavior
    {
        private TreeView _treeView;
        private DroppableAdorner _lastItemAdorner;

        protected override void OnAttached()
        {
            base.OnAttached();

            _treeView = AssociatedObject as TreeView;

            if (_treeView == null)
                Adorner = new TreeViewItemAdorner(AssociatedObject);
        }

        protected override void OnDragEnter(object sender, DragEventArgs e)
        {
            base.OnDragEnter(sender, e);

            if (_treeView != null)
            {
                var lastItem = _treeView.LastItem();

                if (!Equals(_lastItemAdorner?.AdornedElement, lastItem))
                    _lastItemAdorner = new TreeViewLastItemAdorner(lastItem);
            }
        }
        protected override void OnDragOver(object sender, DragEventArgs e)
        {
            base.OnDragOver(sender, e);

            _lastItemAdorner?.Update(DropType.Below, null, null);
        }
        protected override void OnDrop(object sender, DragEventArgs e)
        {
            base.OnDrop(sender, e);

            _lastItemAdorner?.Remove();
        }
        protected override void OnDragLeave(object sender, DragEventArgs e)
        {
            base.OnDragLeave(sender, e);

            _lastItemAdorner?.Remove();
        }
    }
}
