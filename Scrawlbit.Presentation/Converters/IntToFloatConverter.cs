using System;
using System.Globalization;

namespace Scrawlbit.Presentation.Converters
{
    public class IntToFloatConverter : ConverterMarkupExtension
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? intValue = value as int?;

            if (intValue != null)
                return (float)intValue;

            return null;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float? floatValue = value as float?;

            if (floatValue != null)
                return (int)floatValue;

            return null;
        }
    }
}
