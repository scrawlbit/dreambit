using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels;
using DreamBit.Game.Elements;
using DreamBit.General.State;
using Scrawlbit.Presentation.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DreamBit.Extension.Windows
{
    public partial class SceneHierarchyView
    {
        private readonly IStateManager _stateManager;
        private readonly SceneHierarchyViewModel _viewModel;

        public SceneHierarchyView()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            DreamBitPackage.Container.Inject(out _stateManager);

            _viewModel = LoadViewModel<SceneHierarchyViewModel>();
        }

        private void OnPreviewSelectionChanged(object sender, PreviewSelectionChangedEventArgs e)
        {
            if (Keyboard.PrimaryDevice.IsKeyDown(Key.L))
                e.CancelThis = true;
        }
        private void OnTextChanged(object sender, ValueChangedEventArgs<string> e)
        {
            FrameworkElement element = (FrameworkElement)e.OriginalSource;
            GameObject gameObject = (GameObject)element.DataContext;

            string description = $"{e.OldValue} renamed to {e.NewValue}";
            IStateCommand command = gameObject.State().SetProperty(g => g.Name, e, description);

            _stateManager.Add(command);
        }
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2 && ObjectsTree.IsKeyboardFocusWithin)
                ObjectsTree.FindTreeViewItem(_viewModel.Editor.SelectedObject).Focus();
        }
    }
}