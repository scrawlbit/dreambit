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
            IRemoveGameComponentCommand removeGameComponentCommand)
        {
            Editor = editor;
            State = state;
            DropOnInspectCommand = dropOnInspectCommand;
            RemoveGameComponentCommand = removeGameComponentCommand;
        }

        public IEditor Editor { get; }
        public IStateManager State { get; }
        public ICommand DropOnInspectCommand { get; }
        public ICommand RemoveGameComponentCommand { get; }
    }
}
