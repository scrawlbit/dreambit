using System;
using System.Globalization;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Bindings
{
    public class EnumCollectionBinding : Binding, IValueConverter
    {
        public EnumCollectionBinding()
        {
            Converter = this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value as Type;
            if (type?.IsEnum == true)
                return Enum.GetValues(type);

            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
