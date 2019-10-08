using System.IO;
using DreamBit.Extension.Commands;
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

    internal class ProjectManager : IProjectManager
    {
        private readonly IPackageBridge _package;
        private readonly IFileManager _fileManager;
        private readonly IProject _project;
        private readonly IPipeline _pipeline;
        private readonly IBuildContentCommand _buildContentCommand;

        public ProjectManager(
            IPackageBridge package,
            IFileManager fileManager,
            IProject project,
            IPipeline pipeline,
            IBuildContentCommand buildContentCommand)
        {
            _package = package;
            _fileManager = fileManager;
            _project = project;
            _pipeline = pipeline;
            _buildContentCommand = buildContentCommand;
        }

        public void Initialize()
        {
            _package.Monitor.SolutionOpened += OnSolutionOpened;
            _package.Monitor.SolutionClosed += OnSolutionClosed;
            _package.Monitor.ItemsAdded += OnItemsAdded;
            _package.Monitor.ItemsRemoved += OnItemsRemoved;
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
        private void OnItemsAdded(string[] paths)
        {
            foreach (var path in paths)
                _project.AddFile(path);

            _project.Save();
            _pipeline.Save();
            _buildContentCommand.Execute();
        }
        private void OnItemsRemoved(string[] paths)
        {
            foreach (var path in paths)
                _project.RemoveFile(path);

            _project.Save();
            _pipeline.Save();
            _buildContentCommand.Execute(null);
        }
    }
}