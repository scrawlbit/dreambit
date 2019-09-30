using System;
using System.Linq;
using System.Text;
using DreamBit.Pipeline.Imports;
using Microsoft.Xna.Framework;

namespace DreamBit.Pipeline.Translators.ImportTranslators
{
    internal interface ITextureImportTranslator : IObjectTranslator { }

    internal class TextureImportTranslator : ITextureImportTranslator
    {
        public bool TryRead(string text, out object value)
        {
            text = Translations.ReadImporter("TextureImporter", text);

            if (text != null)
            {
                value = new TextureImport(Translations.ReadStringProperty("build", text))
                {
                    ColorKeyColor = ReadColor(text, "processorParam:ColorKeyColor"),
                    ColorKeyEnabled = Translations.ReadBoolProcessorParam("ColorKeyEnabled", text),
                    GenerateMipmaps = Translations.ReadBoolProcessorParam("GenerateMipmaps", text),
                    PremultiplyAlpha = Translations.ReadBoolProcessorParam("PremultiplyAlpha", text),
                    ResizeToPowerOfTwo = Translations.ReadBoolProcessorParam("ResizeToPowerOfTwo", text),
                    MakeSquare = Translations.ReadBoolProcessorParam("MakeSquare", text),
                    TextureFormat = Translations.ReadEnumProcessorParam<TextureFormat>("TextureFormat", text)
                };

                return true;
            }

            value = null;
            return false;
        }
        public bool TryWrite(object value, out string text)
        {
            if (value is TextureImport import)
            {
                var builder = new StringBuilder();

                builder.AppendLine(Translations.WriteBegin(import));
                builder.AppendLine(Translations.WriteImporter("TextureImporter"));
                builder.AppendLine(Translations.WriteProcessor("TextureProcessor"));
                builder.AppendLine(Translations.WriteProcessorParam("ColorKeyColor", WriteColor(import.ColorKeyColor)));
                builder.AppendLine(Translations.WriteProcessorParam("ColorKeyEnabled", import.ColorKeyEnabled));
                builder.AppendLine(Translations.WriteProcessorParam("GenerateMipmaps", import.GenerateMipmaps));
                builder.AppendLine(Translations.WriteProcessorParam("PremultiplyAlpha", import.PremultiplyAlpha));
                builder.AppendLine(Translations.WriteProcessorParam("ResizeToPowerOfTwo", import.ResizeToPowerOfTwo));
                builder.AppendLine(Translations.WriteProcessorParam("MakeSquare", import.MakeSquare));
                builder.AppendLine(Translations.WriteProcessorParam("TextureFormat", import.TextureFormat));
                builder.Append(Translations.WriteBuild(import));

                text = builder.ToString();
                return true;
            }

            text = null;
            return false;
        }

        private static Color ReadColor(string text, string property)
        {
            var value = Translations.ReadStringProperty(property, text);
            var parts = value.Split(',').Select(v => Convert.ToInt32(v.Trim())).ToArray();

            return new Color(parts[0], parts[1], parts[2], parts[3]);
        }
        private static string WriteColor(Color color)
        {
            return $"{color.R},{color.G},{color.B},{color.A}";
        }
    }
}