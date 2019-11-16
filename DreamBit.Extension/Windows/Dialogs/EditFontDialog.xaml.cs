using DreamBit.Extension.Components;
using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels.Dialogs;
using DreamBit.Pipeline.Files;

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
            _viewModel.Create(hierarchy);

            Title = "New Font";
            ShowModal();
        }
        public void EditFont(PipelineFont font)
        {
            _viewModel.Edit(font);

            Title = "Edit Font";
            ShowModal();
        }
    }
}
