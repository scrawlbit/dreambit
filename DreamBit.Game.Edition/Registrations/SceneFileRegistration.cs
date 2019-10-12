using DreamBit.Game.Files;
using DreamBit.Modularization.Management;
using DreamBit.Pipeline;
using DreamBit.Project;
using DreamBit.Project.Registrations;
using Scrawlbit.Json;
using System;

namespace DreamBit.Game.Registrations
{
    internal interface ISceneFileRegistration : IFileRegistration { }
    internal class SceneFileRegistration : ISceneFileRegistration
    {
        private readonly IPipeline _pipeline;
        private readonly IFileManager _fileManager;
        private readonly IJsonParser _jsonParser;

        public SceneFileRegistration(IPipeline pipeline, IFileManager fileManager, IJsonParser jsonParser)
        {
            _pipeline = pipeline;
            _fileManager = fileManager;
            _jsonParser = jsonParser;
        }

        public string Type => "Scene";
        public string Extension => ".scene";
        public Type ObjectType => typeof(SceneFile);

        public ProjectFile CreateInstance() => new SceneFile(_pipeline, _fileManager, _jsonParser);
    }
}
