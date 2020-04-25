using System;
using System.ComponentModel;
using DreamBit.Project.Exceptions;
using Scrawlbit.Notification;
using _Path = System.IO.Path;

namespace DreamBit.Project
{
    public interface IProjectFile : INotifyPropertyChanged
    {
        Guid Id { get; }
        string Name { get; }
        string FileName { get; }
        string Location { get; }
        string Path { get; }
        string Folder { get; }
        string Type { get; }
    }

    public abstract class ProjectFile : NotificationObject, IProjectFile
    {
        private string _location;
        private string _path;

        protected ProjectFile()
        {
            Id = Guid.NewGuid();
        }

        internal IProject Project { get; set; }
        public Guid Id { get; internal set; }
        public string Name => _Path.GetFileNameWithoutExtension(Location);
        public string FileName => _Path.GetFileName(Location);
        public string Location
        {
            get => _location;
            internal set
            {
                ValidateExtension(value);

                _location = value;
                _path = _Path.Combine(Project.Folder, value);

                NotifyAll();
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

                NotifyAll();
            }
        }
        public string Folder => _Path.GetDirectoryName(Path);
        public string Type { get; internal set; }
        public string Extension { get; internal set; }

        internal protected virtual void OnAdded() { }
        internal protected virtual void OnReplaced() { }
        internal protected virtual void OnMoved(MovedEventArgs e) { }
        internal protected virtual void OnRemoved() { }

        private void ValidateExtension(string path)
        {
            if (!path.EndsWith(Extension))
                throw new InvalidFileExtensionException(path, Extension);
        }

        private void NotifyAll()
        {
            OnPropertyChanged(nameof(Location));
            OnPropertyChanged(nameof(Path));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(Folder));
        }
    }
}