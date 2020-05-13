using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Converters
{
    public class ValueConverterGroup : List<object>, IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var converter in this.OfType<IValueConverter>())
                value = converter.Convert(value, targetType, parameter, culture);

            return value;
        }
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object value = values;

            foreach (var converter in this)
            {
                if (converter is IMultiValueConverter multiConverter)
                    value = multiConverter.Convert((object[])value, targetType, parameter, culture);
                else if (converter is IValueConverter valueConverter)
                    value = valueConverter.Convert(value, targetType, parameter, culture);
            }

            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}