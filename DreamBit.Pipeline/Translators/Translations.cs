using System;
using System.Text.RegularExpressions;
using DreamBit.Pipeline.Imports;

namespace DreamBit.Pipeline.Translators
{
    internal static class Translations
    {
        internal static string ReadArea(string name, string text)
        {
            var pattern = $"^#-* {name} -*#(.*?)^(?:(?:#-)|$)";
            var options = RegexOptions.Multiline | RegexOptions.Singleline;
            var match = Regex.Match(text, pattern, options);
            
            return match.Groups[1].Value.Trim();
        }
        internal static string ReadImporter(string importer, string text)
        {
            var pattern = $"^#begin.*\r\n/importer:{importer}\r\n((?:.*\r\n)+?/build:.*)$";
            var match = Regex.Match(text, pattern, RegexOptions.Multiline);

            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        internal static string ReadStringProperty(string property, string text)
        {
            var divisor = !property.Contains(":") ? ":" : "=";
            var pattern = $"^/{property}{divisor}(.*?)\r?\n?$";
            var match = Regex.Match(text, pattern, RegexOptions.Multiline);

            return match.Groups[1].Value.Trim();
        }
        internal static T ReadEnumProperty<T>(string property, string text)
        {
            return (T)Enum.Parse(typeof(T), ReadStringProperty(property, text));
        }
        internal static bool ReadBoolProperty(string property, string text)
        {
            return bool.Parse(ReadStringProperty(property, text));
        }

        internal static string ReadStringProcessorParam(string property, string text)
        {
            return ReadStringProperty("processorParam:" + property, text);
        }
        internal static T ReadEnumProcessorParam<T>(string property, string text)
        {
            return ReadEnumProperty<T>("processorParam:" + property, text);
        }
        internal static bool ReadBoolProcessorParam(string property, string text)
        {
            return ReadBoolProperty("processorParam:" + property, text);
        }

        internal static string WriteBegin(IContentImport import)
        {
            return $"#begin {import.Path}";
        }
        internal static string WriteBuild(IContentImport import)
        {
            return $"/build:{import.Path}";
        }
        internal static string WriteImporter(string importer)
        {
            return WriteProperty("importer", importer);
        }
        internal static string WriteProcessor(string processor)
        {
            return WriteProperty("processor", processor);
        }
        internal static string WriteProcessorParam(string property, object value)
        {
            return WriteProperty("processorParam:" + property, value);
        }
        internal static string WriteProperty(string property, object value)
        {
            var divisor = !property.Contains(":") ? ":" : "=";

            return $"/{property}{divisor}{value}";
        }
    }
}