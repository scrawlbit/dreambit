using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Converters
{
    public class ValueConverterGroup : List<IValueConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var converter in this)
                value = converter.Convert(value, targetType, parameter, culture);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}