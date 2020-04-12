using Scrawlbit.Presentation.Helpers;
using System;
using System.Globalization;

namespace Scrawlbit.Presentation.Converters
{
    public class EnumDisplayNameConverter : ConverterMarkupExtension
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Enum)?.DisplayName();
        }
    }
}