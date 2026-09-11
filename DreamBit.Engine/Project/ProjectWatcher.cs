using System;
using System.IO;

namespace DreamBit.Engine.Project
{
    /// <summary>
    /// Observa a pasta do projeto e avisa quando cenas (.dbscene) são criadas,
    /// removidas ou renomeadas. Substitui os hooks do Solution Explorer do VSIX
    /// por um FileSystemWatcher próprio.
    /// </summary>
    public sealed class ProjectWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;

        public event Action? ScenesChanged;

        public ProjectWatcher(string folder)
        {
            _watcher = new FileSystemWatcher(folder)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;
            _watcher.Renamed += OnChanged;
        }

        private void OnChanged(object sender, FileSystemEventArgs e) => ScenesChanged?.Invoke();

        public void Dispose()
        {
            _watcher.Created -= OnChanged;
            _watcher.Deleted -= OnChanged;
            _watcher.Renamed -= OnChanged;
            _watcher.Dispose();
        }
    }
}
