using System;
using System.Globalization;
using System.Windows.Data;

namespace ScrawlBit.Presentation.Converters
{
    public class CollectionViewSourceConverter : IValueConverter
    {
        private readonly CollectionViewSource _viewSource;

        public CollectionViewSourceConverter()
        {
            _viewSource = new CollectionViewSource();
        }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _viewSource.Source = value;

            return _viewSource.View;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public event FilterEventHandler Filter
        {
            add { _viewSource.Filter += value; }
            remove { _viewSource.Filter -= value; }
        }
    }
}