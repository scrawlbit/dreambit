using System;
using System.IO;

namespace DreamBit.Engine.Project
{
    /// <summary>
    /// Observa a pasta do projeto e avisa quando cenas (.dbscene) são criadas,
    /// removidas ou renomeadas (<see cref="ScenesChanged"/>) e quando um asset
    /// (imagem, tilemap ou som) é escrito no disco (<see cref="AssetChanged"/>,
    /// para hot-reload). Substitui os hooks do Solution Explorer do VSIX por um
    /// FileSystemWatcher próprio.
    /// </summary>
    public sealed class ProjectWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;

        /// <summary>Estrutura de cenas mudou (arquivo criado/removido/renomeado).</summary>
        public event Action? ScenesChanged;

        /// <summary>Um asset foi escrito no disco; o argumento é o caminho completo.</summary>
        public event Action<string>? AssetChanged;

        public ProjectWatcher(string folder)
        {
            _watcher = new FileSystemWatcher(folder)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnStructureChanged;
            _watcher.Deleted += OnStructureChanged;
            _watcher.Renamed += OnStructureChanged;
            _watcher.Changed += OnContentChanged;
        }

        private void OnStructureChanged(object sender, FileSystemEventArgs e)
        {
            ScenesChanged?.Invoke();
            if (IsAssetFile(e.FullPath))
                AssetChanged?.Invoke(e.FullPath);
        }

        private void OnContentChanged(object sender, FileSystemEventArgs e)
        {
            if (IsAssetFile(e.FullPath))
                AssetChanged?.Invoke(e.FullPath);
        }

        /// <summary>True para arquivos de asset que o editor recarrega ao vivo (imagem/tilemap/som).</summary>
        public static bool IsAssetFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" => true, // imagens
                ".tmx" or ".tsx" => true,                                 // tilemaps Tiled
                ".wav" => true,                                           // sons
                _ => false
            };
        }

        public void Dispose()
        {
            _watcher.Created -= OnStructureChanged;
            _watcher.Deleted -= OnStructureChanged;
            _watcher.Renamed -= OnStructureChanged;
            _watcher.Changed -= OnContentChanged;
            _watcher.Dispose();
        }
    }
}
