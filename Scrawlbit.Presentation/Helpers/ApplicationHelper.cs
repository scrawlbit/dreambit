using System.ComponentModel;
using System.Windows;

namespace Scrawlbit.Presentation.Helpers
{
    public static class ApplicationHelper
    {
        public static bool IsInDesignMode()
        {
            return (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
        }

        public static T FindResource<T>(string name)
        {
            return (T)Application.Current.FindResource(name);
        }
    }
}