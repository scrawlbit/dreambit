using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Converters
{
    public class ValueConverterGroup : ConverterMarkupExtension, ICollection<object>
    {
        private readonly ICollection<object> _converters = new List<object>();

        public int Count => _converters.Count;
        public bool IsReadOnly => _converters.IsReadOnly;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var converter in this.OfType<IValueConverter>())
                value = converter.Convert(value, targetType, parameter, culture);

            return value;
        }
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
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

        public void Add(object item)
        {
            if (item is IValueConverter || item is IMultiValueConverter)
                _converters.Add(item);
        }
        public bool Remove(object item)
        {
            return _converters.Remove(item);
        }
        public void Clear()
        {
            _converters.Clear();
        }
        public bool Contains(object item)
        {
            return _converters.Contains(item);
        }
        public void CopyTo(object[] array, int arrayIndex)
        {
            _converters.CopyTo(array, arrayIndex);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _converters.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}