using Scrawlbit.Presentation.Converters;
using System;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace DreamBit.Extension.Converters
{
    public class IconSourceConverter : ConverterMarkupExtension
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(DreamBitPackage.GetResourceUri($"Resources/Icons/{parameter}/{value}.png"));
        }
    }
}
