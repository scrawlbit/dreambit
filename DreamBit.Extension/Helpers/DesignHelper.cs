using System.ComponentModel;
using System.Windows;

namespace DreamBit.Extension.Helpers
{
    public static class DesignHelper
    {
        public static bool IsInDesignMode(this DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(DesignerProperties.IsInDesignModeProperty);
        }
    }
}