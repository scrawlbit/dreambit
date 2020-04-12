using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Scrawlbit.Presentation.Converters
{
    public abstract class ConverterMarkupExtension : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        private readonly IDictionary<Type, object> _converters;

        public ConverterMarkupExtension()
        {
            _converters = new Dictionary<Type, object>();
        }

        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            Type type = GetType();

            if (!_converters.ContainsKey(type))
                _converters[type] = Activator.CreateInstance(type);

            return _converters[type];
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}