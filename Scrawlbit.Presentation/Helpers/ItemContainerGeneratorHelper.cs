using System.Windows;
using System.Windows.Controls;

namespace ScrawlBit.Presentation.Helpers
{
    public static class ItemContainerGeneratorHelper
    {
        public static DependencyObject SafeContainerFromIndex(this ItemContainerGenerator itemContainerGenerator, int index)
        {
            if (index < 0 || index == itemContainerGenerator.Items.Count)
                return null;

            return itemContainerGenerator.ContainerFromIndex(index);
        }
    }
}