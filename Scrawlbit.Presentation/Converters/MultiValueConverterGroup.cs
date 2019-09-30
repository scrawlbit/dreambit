using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Converters
{
    public class MultiValueConverterGroup : List<object>, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object value = values;

            foreach (var converter in this)
            {
                if (converter is IMultiValueConverter)
                    value = ((IMultiValueConverter)converter).Convert((object[])value, targetType, parameter, culture);
                else if (converter is IValueConverter)
                    value = ((IValueConverter)converter).Convert(value, targetType, parameter, culture);
            }

            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}