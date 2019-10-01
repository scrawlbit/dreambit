using System.IO;
using DreamBit.Extension.Components;
using DreamBit.Modularization.Management;
using DreamBit.Pipeline;
using DreamBit.Project;

namespace DreamBit.Extension.Management
{
    public interface IProjectManager
    {
        void Initialize();
    }

    public class ProjectManager : IProjectManager
    {
        private readonly IPackageBridge _package;
        private readonly IFileManager _fileManager;
        private readonly IProject _project;
        private readonly IPipeline _pipeline;

        public ProjectManager(
            IPackageBridge package,
            IFileManager fileManager,
            IProject project,
            IPipeline pipeline)
        {
            _package = package;
            _fileManager = fileManager;
            _project = project;
            _pipeline = pipeline;
        }

        public void Initialize()
        {
            _package.Monitor.SolutionOpened += OnSolutionOpened;
            _package.Monitor.SolutionClosed += OnSolutionClosed;
        }

        private void OnSolutionOpened(string path)
        {
            var directory = new DirectoryInfo(path);
            var file = Path.Combine(directory.FullName, $"{directory.Name}.Content", "Game.dream");

            if (_fileManager.FileExists(file) && !_project.Loaded)
            {
                _project.Load(file);
                _pipeline.Load();
            }
        }
        private void OnSolutionClosed()
        {
            _project.Unload();
            _pipeline.Unload();
        }
    }
}