using DreamBit.Extension.Resources;
using DreamBit.Extension.ViewModels;
using System.Windows.Controls;

namespace DreamBit.Extension.Controls
{
    public class WindowView : UserControl
    {
        public WindowView()
        {
            this.ApplyTheme();
        }

        protected T LoadViewModel<T>() where T : BaseViewModel
        {
            var viewModel = DreamBitPackage.Container.Resolve<T>();
            DataContext = viewModel;

            return viewModel;
        }
        protected void LoadViewModel<T>(out T viewModel) where T : BaseViewModel
        {
            viewModel = LoadViewModel<T>();
        }
    }
}