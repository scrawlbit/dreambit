using DreamBit.Extension.ViewModels;
using Microsoft.VisualStudio.PlatformUI;

namespace DreamBit.Extension.Controls
{
    public class DialogView : DialogWindow
    {
        protected T LoadViewModel<T>() where T : class, IViewModel
        {
            var viewModel = DreamBitPackage.Container.Resolve<T>();
            DataContext = viewModel;

            return viewModel;
        }
        protected void LoadViewModel<T>(out T viewModel) where T : class, IViewModel
        {
            viewModel = LoadViewModel<T>();
        }
    }
}