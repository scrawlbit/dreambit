using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DreamBit.Engine.Diagnostics;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>Cor de cada linha do console conforme o nível do log.</summary>
    public sealed class LogLevelBrushConverter : IValueConverter
    {
        public static readonly LogLevelBrushConverter Instance = new();

        private static readonly IBrush Info = new SolidColorBrush(Color.FromRgb(0xB9, 0xC0, 0xCE));
        private static readonly IBrush Warning = new SolidColorBrush(Color.FromRgb(0xE6, 0xC0, 0x6C));
        private static readonly IBrush Error = new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x6C));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value switch
            {
                LogLevel.Error => Error,
                LogLevel.Warning => Warning,
                _ => Info
            };

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

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
