using System.Windows.Controls;
using TreeView = System.Windows.Controls.MultiSelectTreeView;
using TreeViewItem = System.Windows.Controls.MultiSelectTreeViewItem;

namespace Scrawlbit.Presentation.Helpers
{
    public static class TreeViewHelper
    {
        public static TreeViewItem FindTreeViewItem(this TreeView treeView, object data)
        {
            return FindTreeViewItem(treeView.ItemContainerGenerator, data);
        }
        private static TreeViewItem FindTreeViewItem(ItemContainerGenerator itemContainerGenerator, object data)
        {
            var item = (TreeViewItem)itemContainerGenerator.ContainerFromItem(data);
            if (item != null)
                return item;

            for (var i = 0; i < itemContainerGenerator.Items.Count; i++)
            {
                item = (TreeViewItem)itemContainerGenerator.ContainerFromIndex(i);
                item = FindTreeViewItem(item.ItemContainerGenerator, data);
                if (item != null)
                    return item;
            }

            return null;
        }

        public static TreeViewItem LastItem(this TreeView treeView)
        {
            return (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromIndex(treeView.Items.Count - 1);
        }

        public static TreeView ParentTreeView(this TreeViewItem child)
        {
            return child.ParentsUntil<TreeView>();
        }
        public static TreeViewItem ParentItem(this TreeViewItem child)
        {
            return child.ParentsUntil<TreeViewItem>(o => o is TreeView);
        }
    }
}