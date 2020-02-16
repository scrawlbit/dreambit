using DreamBit.Pipeline.Files;
using DreamBit.Project;
using DreamBit.Project.Registrations;
using System;

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
        public Type ObjectType => typeof(PipelineImage);

        public bool ShouldIncludeFromExternalAction(string path) => true;
        public ProjectFile CreateInstance() => new PipelineImage(_pipeline);
    }
}