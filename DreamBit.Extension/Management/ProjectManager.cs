using DreamBit.Extension.Commands;
using DreamBit.Extension.Components;
using DreamBit.Modularization.Management;
using DreamBit.Pipeline;
using DreamBit.Project;
using System.IO;

namespace DreamBit.Extension.Management
{
    public interface IProjectManager
    {
        void Initialize();

        bool IsSingleHierarchySelected(out IHierarchyBridge hierarchy);
        T AddFileOnSelectedPath<T>(IHierarchyBridge hierarchy, string name) where T : ProjectFile;

        void BuildPipeline();
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
            _package.Monitor.ItemsMoved += OnItemsMoved;
            _package.Monitor.ItemsRemoved += OnItemsRemoved;
        }

        public bool IsSingleHierarchySelected(out IHierarchyBridge hierarchy)
        {
            if (_package.IsSingleHierarchySelected(out hierarchy))
                return hierarchy.ProjectFolder == _project.Folder;

            return false;
        }
        public T AddFileOnSelectedPath<T>(IHierarchyBridge hierarchy, string name) where T : ProjectFile
        {
            string folder = hierarchy.IsFolder ? hierarchy.Path : hierarchy.Folder;
            string fileName = Path.Combine(folder, name);
            T file = _project.AddFile<T>(fileName);

            hierarchy.AddItem(file.Path);

            return file;
        }
        public void BuildPipeline()
        {
            _buildContentCommand.Execute(null);
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

            SaveAll();
        }
        private void OnItemsMoved(string[] oldPaths, string[] newPaths)
        {
            for (int i = 0; i < oldPaths.Length; i++)
                _project.MoveFile(oldPaths[i], newPaths[i]);

            SaveAll();
        }
        private void OnItemsRemoved(string[] paths)
        {
            foreach (var path in paths)
                _project.RemoveFile(path);

            SaveAll();
        }

        private void SaveAll()
        {
            _project.Save();
            _pipeline.Save();
            BuildPipeline();
        }
    }
}