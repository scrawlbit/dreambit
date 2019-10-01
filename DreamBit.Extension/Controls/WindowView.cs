using System.Windows.Controls;
using DreamBit.Extension.ViewModels;

namespace DreamBit.Extension.Controls
{
    public class WindowView : UserControl
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