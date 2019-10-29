using DreamBit.Extension.Resources;
using DreamBit.Extension.ViewModels.Dialogs;
using Microsoft.VisualStudio.PlatformUI;
using StartupLocation = System.Windows.WindowStartupLocation;

namespace DreamBit.Extension.Controls
{
    public class DialogView : DialogWindow
    {
        public DialogView()
        {
            HasMaximizeButton = false;
            HasMinimizeButton = false;
            WindowStartupLocation = StartupLocation.CenterOwner;

            this.ApplyTheme();
        }

        protected T LoadViewModel<T>() where T : BaseDialogViewModel
        {
            var viewModel = DreamBitPackage.Container.Resolve<T>();
            DataContext = viewModel;

            viewModel.Closed += Close;

            return viewModel;
        }
        protected void LoadViewModel<T>(out T viewModel) where T : BaseDialogViewModel
        {
            viewModel = LoadViewModel<T>();
        }
    }
}