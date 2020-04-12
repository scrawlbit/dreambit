using DreamBit.Extension.Helpers;
using DreamBit.Extension.Module;

namespace DreamBit.Extension.Windows
{
    public partial class SceneEditorView
    {
        public SceneEditorView()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            GameControl.Module = DreamBitPackage.Container.Resolve<SceneEditorGame>();
        }
    }
}
