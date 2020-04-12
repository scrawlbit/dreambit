using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Scrawlbit.Presentation.Converters
{
    public class EnumerableHasElementsConverter : ConverterMarkupExtension
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IEnumerable<object>)value).Any();
        }
    }
}