using System.Collections.Generic;
using System.Linq;
using DreamBit.Modularization.Management;
using DreamBit.Project.Exceptions;
using DreamBit.Project.Registrations;
using DreamBit.Project.Serialization;
using ScrawlBit.Helpers;
using _Path = System.IO.Path;

namespace DreamBit.Project
{
    public interface IProject
    {
        bool Loaded { get; }
        string Name { get; }
        string Folder { get; }
        string Path { get; }
        IReadOnlyList<IProjectFile> Files { get; }
        IReadOnlyList<IProjectRegistration> Registrations { get; }

        void AddRegistration(IProjectRegistration registration);
        void AddRegistrations(IProjectRegistrationCollection registration);

        IProjectFile AddFile(string path);
        void IncludeFile(IProjectFile file);
        void RenameFile(IProjectFile file, string path);
        void RemoveFile(IProjectFile file);

        void Load(string path);
        void Unload();
        void Save();
    }

    internal class Project : IProject
    {
        private readonly ISerializer _serializer;
        private readonly IFileManager _fileManager;
        private readonly List<IProjectFile> _files;
        private readonly List<IProjectRegistration> _registrations;

        public Project(ISerializer serializer, IFileManager fileManager)
        {
            _serializer = serializer;
            _fileManager = fileManager;
            _files = new List<IProjectFile>();
            _registrations = new List<IProjectRegistration>();
        }

        public bool Loaded { get; private set; }
        public string Name => _Path.GetFileNameWithoutExtension(Path);
        public string Folder => _Path.GetDirectoryName(Path);
        public string Path { get; private set; }
        public IReadOnlyList<IProjectFile> Files
        {
            get
            {
                if (!Loaded)
                    throw new ProjectNotLoadedException();

                return _files;
            }
        }
        public IReadOnlyList<IProjectRegistration> Registrations => _registrations;

        public void AddRegistration(IProjectRegistration registration)
        {
            if (_registrations.Contains(registration)) return;
            if (_registrations.Any(r => r.Type == registration.Type))
                throw new TypeAlreadyRegistredException();

            _registrations.Add(registration);
        }
        public void AddRegistrations(IProjectRegistrationCollection registration)
        {
            registration.Registrations().ForEach(AddRegistration);
        }

        public IProjectFile AddFile(string path)
        {
            if (_files.Any(f => f.Path == path))
                throw new FileLocationAlreadyExistsException();

            var extension = _Path.GetExtension(path);
            var registration = Registrations.Single(r => r.Extension == extension);
            var file = registration.CreateInstance();

            file.Path = path;
            IncludeFile(file);

            return file;
        }
        public void IncludeFile(IProjectFile file)
        {
            if (_files.Contains(file))
                return;

            if ((file as ProjectFile)?.Project != this)
                return;

            if (_files.Any(f => f.Path == file.Path))
                throw new FileLocationAlreadyExistsException();

            _files.InsertOrdered(file, f => f.Location);
        }
        public void RenameFile(IProjectFile file, string path)
        {
            if (!_files.Contains(file))
                return;

            ((ProjectFile)file).Path = path;
            RemoveFile(file);
            IncludeFile(file);
        }
        public void RemoveFile(IProjectFile file)
        {
            _files.Remove(file);
        }

        public void Load(string path)
        {
            if (Loaded)
                throw new ProjectAlreadyLoadedException();

            if (!_fileManager.FileExists(path))
                throw new ProjectFileNotFoundException(path);

            Path = path;
            _serializer.Load(this);
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
            if (!Loaded)
                throw new ProjectNotLoadedException();

            _serializer.Save(this);
        }
    }
}