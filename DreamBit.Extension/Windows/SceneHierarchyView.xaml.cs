using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels;
using DreamBit.Game.Elements;
using DreamBit.General.State;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DreamBit.Extension.Windows
{
    public partial class SceneHierarchyView
    {
        private readonly IStateManager _stateManager;

        public SceneHierarchyView()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            DreamBitPackage.Container.Inject(out _stateManager);
            LoadViewModel<SceneHierarchyViewModel>();
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
    }
}