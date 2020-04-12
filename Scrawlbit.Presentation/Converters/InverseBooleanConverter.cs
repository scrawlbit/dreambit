using System;
using System.Globalization;

namespace Scrawlbit.Presentation.Converters
{
    public class InverseBooleanConverter : ConverterMarkupExtension
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}