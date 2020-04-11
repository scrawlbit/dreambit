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
            IDropOnInspectCommand dropOnInspectCommand)
        {
            Editor = editor;
            State = state;
            DropOnInspectCommand = dropOnInspectCommand;
        }

        public IEditor Editor { get; }
        public IStateManager State { get; set; }
        public ICommand DropOnInspectCommand { get; }
    }
}
