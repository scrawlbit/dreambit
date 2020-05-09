using DreamBit.Extension.Module;

namespace DreamBit.Extension.ViewModels
{
    public class SceneEditorViewModel : BaseViewModel
    {
        public SceneEditorViewModel(SceneEditorGame gameModule)
        {
            GameModule = gameModule;
        }

        public SceneEditorGame GameModule { get; }
    }
}
