using System.Windows.Input;
using DreamBit.Extension.Commands.SceneHierarchy;
using DreamBit.Extension.Models;

namespace DreamBit.Extension.ViewModels
{
    internal class SceneHierarchyViewModel : BaseViewModel
    {
        public SceneHierarchyViewModel(
            IEditingScene scene,
            IAddGameObjectCommand addGameObjectCommand,
            IAddCameraObjectCommand addCameraObjectCommand,
            ICopyGameObjectCommand copyGameObjectCommand,
            IPasteGameObjectCommand pasteGameObjectCommand,
            IRemoveGameObjectCommand removeGameObjectCommand)
        {
            Scene = scene;
            AddGameObjectCommand = addGameObjectCommand;
            AddCameraObjectCommand = addCameraObjectCommand;
            CopyGameObjectCommand = copyGameObjectCommand;
            PasteGameObjectCommand = pasteGameObjectCommand;
            RemoveGameObjectCommand = removeGameObjectCommand;

            Scene.Objects.Add("Camera");

            var player = Scene.Objects.Add("Player");

            player.IsExpanded = true;
            player.IsSelected = true;
            player.Children.Add("Base");

            scene.Objects.Add("Background");
            scene.Objects.Add("Ground");
            scene.Objects.Add("Foreground");
        }

        public IEditingScene Scene { get; }
        public ICommand AddGameObjectCommand { get; }
        public ICommand AddCameraObjectCommand { get; }
        public ICommand CopyGameObjectCommand { get; }
        public ICommand PasteGameObjectCommand { get; }
        public ICommand RemoveGameObjectCommand { get; }
    }
}