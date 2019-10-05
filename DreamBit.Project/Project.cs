using System.Collections.Generic;
using System.Linq;
using DreamBit.Modularization.Management;
using DreamBit.Project.Exceptions;
using DreamBit.Project.Helpers;
using DreamBit.Project.Registrations;
using DreamBit.Project.Serialization;
using Scrawlbit.Helpers;
using _Path = System.IO.Path;

namespace DreamBit.Project
{
    public interface IProject
    {
        bool Loaded { get; }
        string Name { get; }
        string Folder { get; }
        string Path { get; }
        IReadOnlyList<ProjectFile> Files { get; }

        void AddRegistration(IFileRegistration registration);

        void AddFiles(string[] path);
        void MoveFiles(string[] oldPaths, string[] newPaths);
        void RemoveFiles(string[] paths);

        void Load(string path);
        void Unload();
    }

    internal interface IProjectManager
    {
        IReadOnlyList<IFileRegistration> Registrations { get; }

        void IncludeFile(ProjectFile file);
    }

    internal class Project : IProject, IProjectManager
    {
        private readonly ISerializer _serializer;
        private readonly IFileManager _fileManager;
        private readonly List<IFileRegistration> _registrations;
        private readonly List<ProjectFile> _files;

        public Project(ISerializer serializer, IFileManager fileManager)
        {
            _serializer = serializer;
            _fileManager = fileManager;
            _registrations = new List<IFileRegistration>();
            _files = new List<ProjectFile>();
        }

        public bool Loaded { get; private set; }
        public string Name => _Path.GetFileNameWithoutExtension(Path);
        public string Folder => _Path.GetDirectoryName(Path);
        public string Path { get; private set; }
        public IReadOnlyList<ProjectFile> Files
        {
            get
            {
                EnsureProjectLoaded();
                return _files;
            }
        }
        public IReadOnlyList<IFileRegistration> Registrations => _registrations;

        public void AddRegistration(IFileRegistration registration)
        {
            if (_registrations.Contains(registration)) return;
            if (_registrations.Any(r => r.Type == registration.Type))
                throw new TypeAlreadyRegistredException();

            _registrations.Add(registration);
        }

        public void AddFiles(string[] paths)
        {
            EnsureProjectLoaded();

            foreach (var path in paths)
            {
                if (_files.ContainsPath(path))
                    continue;

                IFileRegistration registration = _registrations.DetermineFromPath(path);
                ProjectFile file = registration.CreateInstance();

                file.Project = this;
                file.Path = path;
                _files.InsertOrdered(file, f => f.Location);
            }

            Save();
        }
        public void MoveFiles(string[] oldPaths, string[] newPaths)
        {
            EnsureProjectLoaded();

            for (int i = 0; i < oldPaths.Length; i++)
            {
                string oldPath = oldPaths[i];
                string newPath = newPaths[i];
                ProjectFile file = _files.GetByPath(oldPath);
                ProjectFile existent = _files.GetByPath(newPath);

                if (file == null)
                    continue;

                file.Path = newPath;

                _files.Remove(existent);
                _files.Remove(file);
                _files.InsertOrdered(file, f => f.Location);
            }

            Save();
        }
        public void RemoveFiles(params string[] paths)
        {
            EnsureProjectLoaded();

            foreach (var path in paths)
            {
                ProjectFile file = _files.GetByPath(path);

                _files.Remove(file);
            }

            Save();
        }

        public void IncludeFile(ProjectFile file)
        {
            if (_files.Any(f => f.Path == file.Path))
                throw new FileLocationAlreadyExistsException();

            _files.InsertOrdered(file, f => f.Location);
        }

        public void Load(string path)
        {
            if (Loaded)
                throw new ProjectAlreadyLoadedException();

            if (!_fileManager.FileExists(path))
                throw new ProjectFileNotFoundException(path);

            Path = path;
            _serializer.Load(this, this);
            Loaded = true;
        }
        public void Unload()
        {
            _files.Clear();
            Path = null;
            Loaded = false;
        }

        private void EnsureProjectLoaded()
        {
            if (!Loaded)
                throw new ProjectNotLoadedException();
        }
        private void Save()
        {
            _serializer.Save(this);
        }
    }
}