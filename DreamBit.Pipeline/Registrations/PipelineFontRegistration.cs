using DreamBit.Modularization.Management;
using DreamBit.Pipeline.Files;
using DreamBit.Project;
using DreamBit.Project.Registrations;

namespace DreamBit.Pipeline.Registrations
{
    internal interface IPipelineFontRegistration : IProjectRegistration
    {
    }

    internal class PipelineFontRegistration : IPipelineFontRegistration
    {
        private readonly IFileManager _fileManager;

        public PipelineFontRegistration(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public string Type => PipelineFont.FontType;
        public string Extension => PipelineFont.FontExtension;

        public ProjectFile CreateInstance()
        {
            return new PipelineFont(_fileManager);
        }
    }
}