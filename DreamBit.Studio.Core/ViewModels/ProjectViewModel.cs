using System;
using System.Collections.ObjectModel;
using System.Threading;
using DreamBit.Engine.Notification;
using DreamBit.Engine.Project;

namespace DreamBit.Studio.ViewModels
{
    /// <summary>
    /// Projeto aberto no editor: lista as cenas da pasta e as mantém atualizadas
    /// via ProjectWatcher (FileSystemWatcher), marshalando para a thread de UI.
    /// </summary>
    public sealed class ProjectViewModel : NotificationObject, IDisposable
    {
        private GameProject? _project;
        private ProjectWatcher? _watcher;

        public ObservableCollection<string> Scenes { get; } = new();

        /// <summary>Caminhos completos das imagens (assets) do projeto.</summary>
        public ObservableCollection<string> Assets { get; } = new();

        public GameProject? Project => _project;
        public bool HasProject => _project != null;
        public string Title => _project?.Name ?? "(nenhum projeto)";

        public void Open(GameProject project)
        {
            _watcher?.Dispose();

            _project = project;
            _watcher = new ProjectWatcher(project.Folder);
            _watcher.ScenesChanged += OnScenesChanged;

            RefreshScenes();
            RefreshAssets();
            OnPropertyChanged(nameof(HasProject));
            OnPropertyChanged(nameof(Title));
        }

        public string? ScenePath(string sceneFileName) => _project?.ScenePath(sceneFileName);

        // Capturado na thread de UI; independe de WPF/Avalonia.
        private readonly SynchronizationContext? _sync = SynchronizationContext.Current;

        private void OnScenesChanged()
        {
            if (_sync != null)
                _sync.Post(_ => RefreshAll(), null);
            else
                RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshScenes();
            RefreshAssets();
        }

        public void RefreshScenes()
        {
            Scenes.Clear();
            if (_project == null)
                return;

            foreach (var scene in _project.EnumerateScenes())
                Scenes.Add(scene);
        }

        public void RefreshAssets()
        {
            Assets.Clear();
            if (_project == null)
                return;

            foreach (var asset in _project.EnumerateAssets())
                Assets.Add(asset);
        }

        public void Dispose() => _watcher?.Dispose();
    }
}
