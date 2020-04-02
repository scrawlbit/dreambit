using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels;

namespace DreamBit.Extension.Windows
{
    public partial class SceneInspectView
    {
        public SceneInspectView()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            LoadViewModel<SceneInspectViewModel>();
        }
    }
}
