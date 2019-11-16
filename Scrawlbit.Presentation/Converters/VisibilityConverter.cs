using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, true) ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, Visibility.Visible);
        }
    }
}
