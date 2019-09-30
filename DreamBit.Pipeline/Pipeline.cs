using DreamBit.Pipeline.Exceptions;
using DreamBit.Pipeline.Imports;
using DreamBit.Pipeline.MonoGame;
using DreamBit.Project;
using _Path = System.IO.Path;

namespace DreamBit.Pipeline
{
    public interface IPipeline
    {
        bool Loaded { get; }
        string Path { get; }
        IGlobalProperties GlobalProperties { get; }
        IContents Contents { get; }

        void Load();
        void Unload();
        void Save();
        void Build(string outputPath, bool clean = false);
    }

    internal class Pipeline : IPipeline
    {
        private readonly IProject _project;
        private readonly IPipelineFile _file;
        private readonly IPipelineBuilder _builder;
        private readonly GlobalProperties _globalProperties;
        private readonly Contents _contents;

        public Pipeline(
            IProject project,
            IPipelineFile file,
            IPipelineBuilder builder,
            IContentImporter contentImporter)
        {
            _project = project;
            _file = file;
            _builder = builder;
            _contents = new Contents(contentImporter);
            _globalProperties = new GlobalProperties();
        }

        public bool Loaded { get; private set; }
        public string Path { get; private set; }
        public IGlobalProperties GlobalProperties => _globalProperties;
        public IContents Contents => _contents;

        public void Load()
        {
            if (Loaded)
                throw new PipelineAlreadyLoadedException();

            Unload();
            Path = _Path.Combine(_project.Folder, "Content.mgcb");

            _file.Load(this);
            Loaded = true;
        }
        public void Unload()
        {
            Loaded = false;
            Path = null;

            _globalProperties.Reset();
            _contents.Clear();
        }
        public void Save()
        {
            if (!Loaded)
                throw new PipelineNotLoadedException();

            _file.Save(this);
        }
        public void Build(string outputPath, bool clean = false)
        {
            if (!Loaded)
                throw new PipelineNotLoadedException();

            _builder.Build(this, outputPath, clean);
        }
    }
}