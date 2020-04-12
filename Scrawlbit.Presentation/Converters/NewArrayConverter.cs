using System;
using System.Globalization;
using System.Linq;

namespace Scrawlbit.Presentation.Converters
{
    public class NewArrayConverter : ConverterMarkupExtension
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToArray();
        }
    }
}