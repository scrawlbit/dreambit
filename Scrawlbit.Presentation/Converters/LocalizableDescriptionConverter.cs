using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Scrawlbit.Presentation.Attributes;

namespace Scrawlbit.Presentation.Converters
{
    [ValueConversion(typeof(object), typeof(String))]
    public class LocalizableDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}