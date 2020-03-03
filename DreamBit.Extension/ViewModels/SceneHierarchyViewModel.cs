using DreamBit.Extension.Commands.SceneHierarchy;
using DreamBit.Extension.Management;
using System.Windows.Input;

namespace DreamBit.Extension.ViewModels
{
    internal class SceneHierarchyViewModel : BaseViewModel
    {
        public SceneHierarchyViewModel(
            IEditor editor,
            IAddGameObjectCommand addGameObjectCommand,
            IAddCameraObjectCommand addCameraObjectCommand,
            ICopyGameObjectCommand copyGameObjectCommand,
            IPasteGameObjectCommand pasteGameObjectCommand,
            IRemoveGameObjectCommand removeGameObjectCommand)
        {
            Editor = editor;
            AddGameObjectCommand = addGameObjectCommand;
            AddCameraObjectCommand = addCameraObjectCommand;
            CopyGameObjectCommand = copyGameObjectCommand;
            PasteGameObjectCommand = pasteGameObjectCommand;
            RemoveGameObjectCommand = removeGameObjectCommand;
        }

        public IEditor Editor { get; }
        public ICommand AddGameObjectCommand { get; }
        public ICommand AddCameraObjectCommand { get; }
        public ICommand CopyGameObjectCommand { get; }
        public ICommand PasteGameObjectCommand { get; }
        public ICommand RemoveGameObjectCommand { get; }
    }
}