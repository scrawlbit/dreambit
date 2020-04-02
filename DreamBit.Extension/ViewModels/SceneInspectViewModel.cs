using DreamBit.Extension.Management;

namespace DreamBit.Extension.ViewModels
{
    public class SceneInspectViewModel : BaseViewModel
    {
        public SceneInspectViewModel(IEditor editor)
        {
            Editor = editor;
        }

        public IEditor Editor { get; }
    }
}
