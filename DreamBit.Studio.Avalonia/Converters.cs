using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>Mostra só o nome do arquivo de um caminho completo (lista de assets).</summary>
    public sealed class FileNameConverter : IValueConverter
    {
        public static readonly FileNameConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is string path && path.Length > 0 ? Path.GetFileName(path) : string.Empty;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>True quando a string não está vazia (para exibir a mensagem de erro do script).</summary>
    public sealed class NonEmptyConverter : IValueConverter
    {
        public static readonly NonEmptyConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is string s && !string.IsNullOrWhiteSpace(s);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
