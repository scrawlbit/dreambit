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
        private readonly IPipeline _pipeline;

        public PipelineImageRegistration(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public string Type => "Image";
        public string Extension => ".png";

        public ProjectFile CreateInstance()
        {
            return new PipelineImage(_pipeline);
        }
    }
}