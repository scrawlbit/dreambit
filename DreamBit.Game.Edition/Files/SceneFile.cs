using DreamBit.Game.Elements;
using DreamBit.Modularization.Management;
using DreamBit.Pipeline;
using DreamBit.Project;
using Scrawlbit.Json;
using System.Text;

namespace DreamBit.Game.Files
{
    public sealed class SceneFile : ProjectFile
    {
        private readonly IPipeline _pipeline;
        private readonly IFileManager _fileManager;
        private readonly IJsonParser _jsonParser;
        private readonly Scene _scene;

        internal SceneFile(IPipeline pipeline, IFileManager fileManager, IJsonParser jsonParser)
        {
            _pipeline = pipeline;
            _fileManager = fileManager;
            _jsonParser = jsonParser;
            _scene = new Scene();
        }

        protected override void OnAdded()
        {
            if (!_fileManager.FileExists(Path))
                Save();

            _pipeline.Contents.AddCopy(this);
        }

        private void Save()
        {
            string json = _jsonParser.ParseObject(_scene);

            _fileManager.WriteAllText(Path, json, Encoding.UTF8);
        }
    }
}