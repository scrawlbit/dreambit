using DreamBit.Game.Files;
using DreamBit.Game.Serialization;
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
        private readonly IGameElementsParser _parser;

        public SceneFileRegistration(IPipeline pipeline, IFileManager fileManager, IGameElementsParser parser)
        {
            _pipeline = pipeline;
            _fileManager = fileManager;
            _parser = parser;
        }

        public string Type => "Scene";
        public string Extension => ".scene";
        public Type ObjectType => typeof(SceneFile);

        public bool ShouldIncludeFromExternalAction(string path) => true;
        public ProjectFile CreateInstance() => new SceneFile(_pipeline, _fileManager, _parser);
    }
}
