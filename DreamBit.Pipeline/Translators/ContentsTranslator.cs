using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DreamBit.Pipeline.Imports;
using DreamBit.Pipeline.Translators.ImportTranslators;
using ScrawlBit.Helpers;

namespace DreamBit.Pipeline.Translators
{
    internal interface IContentsTranslator : ITranslator { }

    internal class ContentsTranslator : IContentsTranslator
    {
        private readonly IContentImporter _contentImporter;
        private readonly IObjectTranslator[] _translators;

        public ContentsTranslator(
            IContentImporter contentImporter,
            ICopyImportTranslator copyImportTranslator,
            IFontImportTranslator fontImportTranslator,
            ITextureImportTranslator textureImportTranslator)
        {
            _contentImporter = contentImporter;
            _translators = new IObjectTranslator[]
            {
                copyImportTranslator,
                fontImportTranslator,
                textureImportTranslator
            };
        }

        public void Read(string text)
        {
            text = Translations.ReadArea("Content", text);

            var imports = Regex.Split(text, "#begin ");

            foreach (var import in imports.Where(StringHelper.HasValue))
            {
                foreach (var translator in _translators)
                {
                    if (translator.TryRead($"#begin {import.Trim()}", out var value))
                    {
                        _contentImporter.AddOrUpdate((IContentImport)value);
                        break;
                    }
                }
            }
        }
        public string Write()
        {
            var builder = new StringBuilder();

            builder.AppendLine("#---------------------------------- Content ---------------------------------#");

            foreach (var import in _contentImporter.Imports)
            {
                foreach (var translator in _translators)
                {
                    if (translator.TryWrite(import, out var text))
                    {
                        builder.AppendLine();
                        builder.AppendLine(text);
                    }
                }
            }

            return builder.ToString();
        }
    }
}