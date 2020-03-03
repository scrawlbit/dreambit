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
        private bool _loaded;
        private Scene _scene;

        internal SceneFile(IPipeline pipeline, IFileManager fileManager, IJsonParser jsonParser)
        {
            _pipeline = pipeline;
            _fileManager = fileManager;
            _jsonParser = jsonParser;
        }

        public Scene Scene
        {
            get
            {
                EnsureLoaded();
                return _scene;
            }
        }

        public void Load()
        {
            string json = _fileManager.ReadAllText(Path);

            _scene = _jsonParser.ParseString<Scene>(json);
            _loaded = true;
        }
        public void Save()
        {
            _scene = _scene ?? new Scene();

            string json = _jsonParser.ParseObject(_scene);

            _fileManager.WriteAllText(Path, json, Encoding.UTF8);
            _loaded = true;
        }

        protected override void OnAdded()
        {
            if (!_fileManager.FileExists(Path))
                Save();

            _pipeline.Contents.AddCopy(this);
        }
        protected override void OnMoved(MovedEventArgs e)
        {
            _pipeline.Contents.Move(this, e.OldLocation);
        }
        protected override void OnRemoved()
        {
            _pipeline.Contents.Remove(this);
        }

        private void EnsureLoaded()
        {
            if (!_loaded)
                Load();
        }
    }
}