using DreamBit.Pipeline.Imports;
using DreamBit.Pipeline.MonoGame;
using DreamBit.Pipeline.Registrations;
using DreamBit.Pipeline.Translators;
using DreamBit.Pipeline.Translators.ImportTranslators;
using DreamBit.Project.Helpers;
using Scrawlbit.Injection.Configuration;

namespace DreamBit.Pipeline.Properties
{
    public class PipelineInjectionModule : IInjectionModule
    {
        private IRegistrationBuilder _builder;

        public void Register(IRegistrationBuilder builder)
        {
            _builder = builder;

            Imports();
            MonoGame();
            Pipeline();
            Registrations();
            Translators();
        }

        private void Imports()
        {
            _builder.Register<IContentImporter>().Singleton<ContentImporter>();
        }
        private void MonoGame()
        {
            _builder.Register<IPipelineBuilder>().Singleton<PipelineBuilder>();
            _builder.Register<IPipelineFile>().Singleton<PipelineFile>();
        }
        private void Pipeline()
        {
            _builder.Register<IPipeline>().Singleton<Pipeline>();
        }
        private void Registrations()
        {
            _builder.RegisterFile<IPipelineFontRegistration>().Transient<PipelineFontRegistration>();
            _builder.RegisterFile<IPipelineImageRegistration>().Transient<PipelineImageRegistration>();
        }
        private void Translators()
        {
            _builder.Register<ICopyImportTranslator>().Transient<CopyImportTranslator>();
            _builder.Register<IFontImportTranslator>().Transient<FontImportTranslator>();
            _builder.Register<ITextureImportTranslator>().Transient<TextureImportTranslator>();

            _builder.Register<IContentsTranslator>().Transient<ContentsTranslator>();
            _builder.Register<IGlobalPropertiesTranslator>().Transient<GlobalPropertiesTranslator>();
            _builder.Register<IReferencesTranslator>().Transient<ReferencesTranslator>();
        }
    }
}