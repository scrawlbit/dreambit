using DreamBit.Extension.Management;
using DreamBit.General.State;

namespace DreamBit.Extension.ViewModels
{
    public class SceneInspectViewModel : BaseViewModel
    {
        public SceneInspectViewModel(IEditor editor, IStateManager state)
        {
            Editor = editor;
            State = state;
        }

        public IEditor Editor { get; }
        public IStateManager State { get; set; }
    }
}
