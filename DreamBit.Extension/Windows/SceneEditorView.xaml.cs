using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels;

namespace DreamBit.Extension.Windows
{
    public partial class SceneEditorView
    {
        public SceneEditorView()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            LoadViewModel<SceneEditorViewModel>();
        }
    }
}
