using DreamBit.Extension.Commands.SceneInspect;
using DreamBit.Extension.Management;
using DreamBit.General.State;
using System.Windows.Input;

namespace DreamBit.Extension.ViewModels
{
    public class SceneInspectViewModel : BaseViewModel
    {
        public SceneInspectViewModel(
            IEditor editor,
            IStateManager state,
            IDropOnInspectCommand dropOnInspectCommand,
            IRemoveGameObjectComponentCommand removeGameObjectComponentCommand)
        {
            Editor = editor;
            State = state;
            DropOnInspectCommand = dropOnInspectCommand;
            RemoveGameObjectComponentCommand = removeGameObjectComponentCommand;
        }

        public IEditor Editor { get; }
        public IStateManager State { get; }
        public ICommand DropOnInspectCommand { get; }
        public ICommand RemoveGameObjectComponentCommand { get; }
    }
}
