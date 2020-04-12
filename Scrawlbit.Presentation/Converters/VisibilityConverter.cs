using System;
using System.Globalization;
using System.Windows;

namespace Scrawlbit.Presentation.Converters
{
    public class VisibilityConverter : ConverterMarkupExtension
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, true) ? Visibility.Visible : Visibility.Collapsed;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, Visibility.Visible);
        }
    }
}
