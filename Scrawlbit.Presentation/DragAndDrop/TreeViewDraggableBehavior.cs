using System.Windows;
using System.Windows.Controls;
using Scrawlbit.Presentation.Helpers;

namespace Scrawlbit.Presentation.DragAndDrop
{
    public class TreeViewDraggableBehavior : DraggableBehavior
    {
        private TreeView _treeView;
        private TreeViewItem _item;

        protected override void OnAttached()
        {
            base.OnAttached();

            _item = AssociatedObject.ParentsUntil<TreeViewItem>();
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