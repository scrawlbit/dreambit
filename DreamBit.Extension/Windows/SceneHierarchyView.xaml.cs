using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels;

namespace DreamBit.Extension.Windows
{
    public partial class SceneHierarchyView
    {
        public SceneHierarchyView()
        {
            InitializeComponent();
            if (this.IsInDesignMode()) return;

            LoadViewModel<SceneHierarchyViewModel>();
        }
    }
}