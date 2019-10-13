using System.Text;
using System.Text.RegularExpressions;
using DreamBit.Pipeline.Imports;

namespace DreamBit.Pipeline.Translators.ImportTranslators
{
    internal interface ICopyImportTranslator : IObjectTranslator { }

    internal class CopyImportTranslator : ICopyImportTranslator
    {
        public bool TryRead(string text, out object value)
        {
            var pattern = $"^#begin (.*)\r\n/copy:.*$";
            var match = Regex.Match(text, pattern, RegexOptions.Multiline);

            if (match.Success)
            {
                value = new CopyImport(match.Groups[1].Value);
                return true;
            }

            value = null;
            return false;
        }
        public bool TryWrite(object value, out string text)
        {
            if (value is CopyImport import)
            {
                var builder = new StringBuilder();

                builder.AppendLine(Translations.WriteBegin(import));
                builder.Append($"/copy:{import.Path}");

                text = builder.ToString();
                return true;
            }

            text = null;
            return false;
        }
    }
}