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

        T AddFile<T>(string fileName) where T : ProjectFile;
        ProjectFile AddFile(string path);
        void MoveFile(string oldPath, string newPath);
        void RemoveFile(string path);

        void Load(string path);
        void Unload();
        void Save();
    }

    internal interface IProjectManager
    {
        IFileRegistrations Registrations { get; }

        void IncludeFile(ProjectFile file);
    }

    internal partial class Project : IProject, IProjectManager
    {
        private readonly ISerializer _serializer;
        private readonly IFileManager _fileManager;
        private readonly List<ProjectFile> _files;
        private bool _hasChanges;

        public Project(ISerializer serializer, IFileManager fileManager, IFileRegistrations registrations)
        {
            _serializer = serializer;
            _fileManager = fileManager;
            _files = new List<ProjectFile>();

            Registrations = registrations;
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
        public IFileRegistrations Registrations { get; }

        public T AddFile<T>(string fileName) where T : ProjectFile
        {
            EnsureProjectLoaded();

            IFileRegistration registration = Registrations.DetermineFromObjectType(typeof(T));
            string path = $"{fileName}{registration.Extension}";

            return (T)AddFile(path, registration);
        }
        public ProjectFile AddFile(string path)
        {
            EnsureProjectLoaded();

            return AddFile(path, Registrations.DetermineFromPath(path));
        }
        public void MoveFile(string oldPath, string newPath)
        {
            EnsureProjectLoaded();

            ProjectFile file = _files.GetByPath(oldPath);
            ProjectFile existent = _files.GetByPath(newPath);

            if (file == null)
                return;

            var args = new MovedEventArgs();

            args.OldPath = oldPath;
            args.OldLocation = file.Location;

            file.Path = newPath;

            _files.Remove(existent);
            _files.Remove(file);
            _files.InsertOrdered(file, f => f.Location);

            file.OnMoved(args);
            IndicateChanges();
        }
        public void RemoveFile(string path)
        {
            EnsureProjectLoaded();

            if (!path.StartsWith(Folder)) return;

            ProjectFile file = _files.GetByPath(path);

            if (file == null)
                return;

            _files.Remove(file);

            file.OnRemoved();
            IndicateChanges();
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
        public void Save()
        {
            if (_hasChanges)
            {
                _serializer.Save(this);
                _hasChanges = false;
            }
        }

        private void EnsureProjectLoaded()
        {
            if (!Loaded)
                throw new ProjectNotLoadedException();
        }
        private ProjectFile AddFile(string path, IFileRegistration registration)
        {
            if (!path.StartsWith(Folder))
                return null;

            ProjectFile file = _files.GetByPath(path);

            if (file == null)
            {
                file = registration.CreateInstance();
                file.Project = this;
                file.Type = registration.Type;
                file.Extension = registration.Extension;
                file.Path = path;

                _files.InsertOrdered(file, f => f.Location);

                file.OnAdded();
                IndicateChanges();
            }
            else
            {
                file.OnReplaced();
            }
            
            return file;
        }
        private void IndicateChanges()
        {
            _hasChanges = true;
        }
    }
}