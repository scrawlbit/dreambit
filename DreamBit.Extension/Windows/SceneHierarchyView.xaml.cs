using DreamBit.Extension.Controls.TreeViews;
using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels;
using DreamBit.Game.Elements;
using DreamBit.General.State;
using Scrawlbit.Presentation.DragAndDrop;
using Scrawlbit.Presentation.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace DreamBit.Extension.Windows
{
    public partial class SceneHierarchyView
    {
        private readonly SceneHierarchyViewModel _viewModel;
        private readonly IStateManager _stateManager;

        public SceneHierarchyView()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            _viewModel = LoadViewModel<SceneHierarchyViewModel>();
            DreamBitPackage.Container.Inject(out _stateManager);
        }

        private void OnTextLoaded(object sender, RoutedEventArgs e)
        {
            var editable = (EditableTreeViewItemText)sender;
            var border = editable.ParentsUntil<Border>();

            border.AddBehavior<TreeViewDraggableBehavior>();
            border.AddBehavior(new ExtendedTreeViewDroppableBehavior
            {
                DropCommand = _viewModel.MoveGameObjectCommand
            });

            editable.Loaded -= OnTextLoaded;
        }
        private void OnTextChanged(object sender, (object OldValue, object NewValue) e)
        {
            var editable = (EditableTreeViewItemText)sender;
            var gameObject = (GameObject)editable.DataContext;

            string description = $"{e.OldValue} renamed to {e.NewValue}";
            IStateCommand command = gameObject.State().SetProperty(g => g.Name, e.OldValue, e.NewValue, description);

            _stateManager.Add(command);
        }
    }
}