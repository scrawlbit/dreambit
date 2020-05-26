using Scrawlbit.Presentation.Helpers;
using System.Windows;
using TreeView = System.Windows.Controls.MultiSelectTreeView;
using TreeViewItem = System.Windows.Controls.MultiSelectTreeViewItem;

namespace Scrawlbit.Presentation.DragAndDrop
{
    public class TreeViewDraggableBehavior : DraggableBehavior
    {
        private TreeView _treeView;
        private TreeViewItem _item;

        protected override void OnAttached()
        {
            base.OnAttached();

            _item = AssociatedObject.ClosestUntil<TreeViewItem>();
            _treeView = _item.ParentTreeView();
        }

        protected override DataObject GetDataObject(object[] data)
        {
            var dataObject = base.GetDataObject(data);

            if (data != null)
            {
                var items = new TreeViewItem[data.Length];

                for (var i = 0; i < data.Length; i++)
                {
                    if (data[i] == AssociatedObject.DataContext)
                        items[i] = _item;
                    else
                        items[i] = _treeView.FindTreeViewItem(data[i]);
                }

                dataObject.SetData(typeof(TreeViewItem[]), items);
            }

            return dataObject;
        }
    }
}