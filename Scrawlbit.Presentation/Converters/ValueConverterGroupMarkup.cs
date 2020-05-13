using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Scrawlbit.Presentation.Converters
{
    public abstract class ValueConverterGroupMarkup : ConverterMarkupExtension, ICollection<object>
    {
        private readonly ValueConverterGroup _group = new ValueConverterGroup();

        public int Count => _group.Count;
        public bool IsReadOnly => ((ICollection<object>)_group).IsReadOnly;

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _group.Convert(value, targetType, parameter, culture);
        }
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return _group.Convert(values, targetType, parameter, culture);
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _group.ConvertBack(value, targetType, parameter, culture);
        }
        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return _group.ConvertBack(value, targetTypes, parameter, culture);
        }

        public void Add(object item) => _group.Add(item);
        public bool Remove(object item) => _group.Remove(item);
        public void Clear() => _group.Clear();
        public bool Contains(object item) => _group.Contains(item);
        public void CopyTo(object[] array, int arrayIndex) => _group.CopyTo(array, arrayIndex);
        public IEnumerator<object> GetEnumerator() => _group.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _group.GetEnumerator();
    }
}