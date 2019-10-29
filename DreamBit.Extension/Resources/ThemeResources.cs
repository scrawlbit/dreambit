using System;
using System.Windows;
using System.Windows.Controls;

namespace DreamBit.Extension.Resources
{
    public static class ThemeResources
    {
        public static void ApplyTheme(this ContentControl control)
        {
            var path = new Uri("pack://application:,,,/DreamBit.Extension;component/Resources/Styles/Theme.xaml");
            var resource = new ResourceDictionary { Source = path };

            control.Resources.MergedDictionaries.Add(resource);
        }
    }
}
