using DreamBit.Modularization.Management;
using DreamBit.Pipeline.Files;
using DreamBit.Project;
using DreamBit.Project.Registrations;
using System;

namespace DreamBit.Pipeline.Registrations
{
    internal interface IPipelineFontRegistration : IFileRegistration
    {
    }

    internal class PipelineFontRegistration : IPipelineFontRegistration
    {
        private readonly IPipeline _pipeline;
        private readonly IFileManager _fileManager;

        public PipelineFontRegistration(IPipeline pipeline, IFileManager fileManager)
        {
            _pipeline = pipeline;
            _fileManager = fileManager;
        }

        public string Type => "Font";
        public string Extension => ".spritefont";
        public Type ObjectType => typeof(PipelineFont);

        public bool ShouldIncludeFromExternalAction(string path) => true;
        public ProjectFile CreateInstance() => new PipelineFont(_pipeline, _fileManager);
    }
}