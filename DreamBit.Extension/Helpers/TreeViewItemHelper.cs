using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DreamBit.Extension.Helpers
{
    public static class TreeViewItemHelper
    {
        public static int GetDepth(this TreeViewItem item)
        {
            return item.GetAncestors().TakeWhile(e => !(e is TreeView)).OfType<TreeViewItem>().Count();
        }

        private static IEnumerable<DependencyObject> GetAncestors(this DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);

            while (parent != null)
            {
                yield return parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }
}
