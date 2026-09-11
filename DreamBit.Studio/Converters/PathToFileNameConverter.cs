using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace DreamBit.Studio.Converters
{
    /// <summary>Caminho completo → nome do arquivo (para exibir na lista de assets).</summary>
    public sealed class PathToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string path ? Path.GetFileName(path) : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value;
    }
}
