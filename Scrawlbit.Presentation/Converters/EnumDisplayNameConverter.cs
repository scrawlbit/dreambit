using System;
using System.Globalization;
using System.Windows.Data;
using ScrawlBit.Presentation.Helpers;

namespace ScrawlBit.Presentation.Converters
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Enum)?.DisplayName();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}