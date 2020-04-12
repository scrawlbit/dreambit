using Scrawlbit.Presentation.Attributes;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class LocalizableDescriptionConverter : ConverterMarkupExtension
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var fi = value.GetType().GetField(value.ToString());

                if (fi != null)
                {
                    var attribute = fi.GetCustomAttribute<LocalizableDescriptionAttribute>();
                    if (attribute != null)
                        return attribute.Description;
                }

                return value.ToString();
            }

            return string.Empty;
        }
    }
}