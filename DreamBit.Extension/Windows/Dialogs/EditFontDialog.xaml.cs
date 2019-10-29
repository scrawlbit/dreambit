using DreamBit.Extension.Components;
using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels.Dialogs;

namespace DreamBit.Extension.Windows.Dialogs
{
    public partial class EditFontDialog
    {
        private readonly EditFontDialogViewModel _viewModel;

        public EditFontDialog()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            LoadViewModel(out _viewModel);
        }

        public void NewFont(IHierarchyBridge hierarchy)
        {
            _viewModel.Hierarchy = hierarchy;

            Title = "New Font";
            ShowModal();
        }
    }
}
