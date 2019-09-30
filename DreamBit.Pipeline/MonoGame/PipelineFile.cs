using System.Text;
using DreamBit.Modularization.Management;
using DreamBit.Pipeline.Translators;

namespace DreamBit.Pipeline.MonoGame
{
    internal interface IPipelineFile
    {
        void Load(IPipeline pipeline);
        void Save(IPipeline pipeline);
    }

    internal class PipelineFile : IPipelineFile
    {
        private readonly IFileManager _fileManager;
        private readonly ITranslator[] _translators;
        private readonly UTF8Encoding _utf8Encoding;

        public PipelineFile(
            IFileManager fileManager,
            IGlobalPropertiesTranslator globalPropertiesTranslator,
            IReferencesTranslator referencesTranslator,
            IContentsTranslator contentsTranslator)
        {
            _fileManager = fileManager;

            _translators = new ITranslator[] {
                globalPropertiesTranslator,
                referencesTranslator,
                contentsTranslator
            };

            _utf8Encoding = new UTF8Encoding(false);
        }

        public void Load(IPipeline pipeline)
        {
            if (_fileManager.FileExists(pipeline.Path))
            {
                var text = _fileManager.ReadAllText(pipeline.Path);

                foreach (var translator in _translators)
                    translator.Read(text);
            }
        }
        public void Save(IPipeline pipeline)
        {
            var builder = new StringBuilder();

            foreach (var translator in _translators)
            {
                builder.AppendLine(translator.Write());
                builder.AppendLine();
            }

            _fileManager.WriteAllText(pipeline.Path, builder.ToString(), _utf8Encoding);
        }
    }
}