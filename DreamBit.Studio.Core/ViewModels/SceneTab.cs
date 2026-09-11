using DreamBit.Engine.Elements;
using DreamBit.Engine.Notification;

namespace DreamBit.Studio.ViewModels
{
    /// <summary>Uma cena aberta em aba no editor.</summary>
    public sealed class SceneTab : NotificationObject
    {
        private bool _isActive;
        private string? _path;

        public SceneTab(Scene scene, string? path = null)
        {
            Scene = scene;
            _path = path;
        }

        public Scene Scene { get; }

        public string? Path
        {
            get => _path;
            set { if (Set(ref _path, value)) OnPropertyChanged(nameof(Title)); }
        }

        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }

        /// <summary>Nome exibido na aba: arquivo (se salvo) ou o nome da cena.</summary>
        public string Title =>
            _path != null ? System.IO.Path.GetFileName(_path) : Scene.Name;
    }
}
