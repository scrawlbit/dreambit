using System;
using DreamBit.Project.Exceptions;
using _Path = System.IO.Path;

namespace DreamBit.Project
{
    public interface IProjectFile
    {
        Guid Id { get; }
        string Name { get; }
        string Location { get; }
        string Path { get; }
        string Type { get; }
    }

    public abstract class ProjectFile : IProjectFile
    {
        private string _location;
        private string _path;

        protected ProjectFile()
        {
            Id = Guid.NewGuid();
        }

        internal IProject Project { get; set; }
        public Guid Id { get; internal set; }
        public string Name => _Path.GetFileName(Location);
        public string Location
        {
            get => _location;
            internal set
            {
                ValidateExtension(value);

                _location = value;
                _path = _Path.Combine(Project.Folder, value);
            }
        }
        public string Path
        {
            get => _path;
            internal set
            {
                ValidateExtension(value);

                _path = value;
                _location = value.Replace($"{Project.Folder}\\", "");
            }
        }
        public string Type { get; internal set; }
        public string Extension { get; internal set; }

        internal protected virtual void OnAdded() { }
        internal protected virtual void OnMoved(MovedEventArgs e) { }
        internal protected virtual void OnRemoved() { }

        private void ValidateExtension(string path)
        {
            if (!path.EndsWith(Extension))
                throw new InvalidFileExtensionException(path, Extension);
        }
    }
}