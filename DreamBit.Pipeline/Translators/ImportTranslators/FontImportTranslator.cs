using System.Text;
using DreamBit.Pipeline.Imports;

namespace DreamBit.Pipeline.Translators.ImportTranslators
{
    internal interface IFontImportTranslator : IObjectTranslator { }

    internal class FontImportTranslator : IFontImportTranslator
    {
        public bool TryRead(string text, out object value)
        {
            text = Translations.ReadImporter("FontDescriptionImporter", text);

            if (text != null)
            {
                value = new FontImport(Translations.ReadStringProperty("build", text))
                {
                    PremultiplyAlpha = Translations.ReadBoolProcessorParam("PremultiplyAlpha", text),
                    TextureFormat = Translations.ReadEnumProcessorParam<TextureFormat>("TextureFormat", text)
                };

                return true;
            }

            value = null;
            return false;
        }
        public bool TryWrite(object value, out string text)
        {
            if (value is FontImport import)
            {
                var builder = new StringBuilder();

                builder.AppendLine(Translations.WriteBegin(import));
                builder.AppendLine(Translations.WriteImporter("FontDescriptionImporter"));
                builder.AppendLine(Translations.WriteProcessor("FontDescriptionProcessor"));
                builder.AppendLine(Translations.WriteProcessorParam("PremultiplyAlpha", import.PremultiplyAlpha));
                builder.AppendLine(Translations.WriteProcessorParam("TextureFormat", import.TextureFormat));
                builder.Append(Translations.WriteBuild(import));

                text = builder.ToString();
                return true;
            }

            text = null;
            return false;
        }
    }
}