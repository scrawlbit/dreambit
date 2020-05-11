using System;
using System.Globalization;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Data
{
    public class ComparisonBinding : MultiBinding
    {
        public ComparisonBinding()
        {
            Converter = new EqualConverter();
        }

        private class EqualConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        if (i != j && !Equals(values[i], values[j]))
                            return false;
                    }
                }

                return true;
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
