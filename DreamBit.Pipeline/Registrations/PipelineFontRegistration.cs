﻿using DreamBit.Modularization.Management;
using DreamBit.Pipeline.Files;
using DreamBit.Project;
using DreamBit.Project.Registrations;

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

        public ProjectFile CreateInstance()
        {
            return new PipelineFont(_pipeline, _fileManager);
        }
    }
}