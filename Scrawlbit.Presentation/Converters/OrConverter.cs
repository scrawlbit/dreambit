using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Converters
{
    public class OrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = values?.Any(v => Equals(v, true)) ?? false;
            return result;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
