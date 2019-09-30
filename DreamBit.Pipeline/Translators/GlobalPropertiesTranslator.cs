using System;
using System.Text;

namespace DreamBit.Pipeline.Translators
{
    internal interface IGlobalPropertiesTranslator : ITranslator { }

    internal class GlobalPropertiesTranslator : IGlobalPropertiesTranslator
    {
        private readonly Lazy<IPipeline> _pipeline;

        public GlobalPropertiesTranslator(Func<IPipeline> pipeline)
        {
            _pipeline = new Lazy<IPipeline>(pipeline);
        }

        private IGlobalProperties GlobalProperties => _pipeline.Value.GlobalProperties;

        public void Read(string text)
        {
            text = Translations.ReadArea("Global Properties", text);

            GlobalProperties.OutputDir = Translations.ReadStringProperty("outputDir", text);
            GlobalProperties.IntermediateDir = Translations.ReadStringProperty("intermediateDir", text);
            GlobalProperties.Platform = Translations.ReadEnumProperty<Platform>("platform", text);
            GlobalProperties.Config = Translations.ReadStringProperty("config", text);
            GlobalProperties.Profile = Translations.ReadEnumProperty<Profile>("profile", text);
            GlobalProperties.Compress = Translations.ReadBoolProperty("compress", text);
        }
        public string Write()
        {
            var builder = new StringBuilder();

            builder.AppendLine("#----------------------------- Global Properties ----------------------------#");
            builder.AppendLine();
            builder.AppendLine(Translations.WriteProperty("outputDir", GlobalProperties.OutputDir));
            builder.AppendLine(Translations.WriteProperty("intermediateDir", GlobalProperties.IntermediateDir));
            builder.AppendLine(Translations.WriteProperty("platform", GlobalProperties.Platform));
            builder.AppendLine(Translations.WriteProperty("config", GlobalProperties.Config));
            builder.AppendLine(Translations.WriteProperty("profile", GlobalProperties.Profile));
            builder.AppendLine(Translations.WriteProperty("compress", GlobalProperties.Compress));

            return builder.ToString();
        }
    }
}