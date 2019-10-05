using DreamBit.Pipeline.Files;
using DreamBit.Project;
using DreamBit.Project.Registrations;

namespace DreamBit.Pipeline.Registrations
{
    internal interface IPipelineImageRegistration : IFileRegistration
    {
    }

    internal class PipelineImageRegistration : IPipelineImageRegistration
    {
        public string Type => PipelineImage.ImageType;
        public string Extension => PipelineImage.ImageExtension;

        public ProjectFile CreateInstance()
        {
            return new PipelineImage();
        }
    }
}