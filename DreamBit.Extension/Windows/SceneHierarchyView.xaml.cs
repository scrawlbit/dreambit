using DreamBit.Extension.Controls.TreeViews;
using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels;
using Scrawlbit.Presentation.DragAndDrop;
using Scrawlbit.Presentation.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace DreamBit.Extension.Windows
{
    public partial class SceneHierarchyView
    {
        private readonly SceneHierarchyViewModel _viewModel;

        public SceneHierarchyView()
        {
            InitializeComponent();
            if (this.IsInDesignMode()) return;

            _viewModel = LoadViewModel<SceneHierarchyViewModel>();
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
            // TODO

            //var editable = (EditableTreeViewItemText)sender;
            //var gameObject = (IGameObject)editable.DataContext;

            //gameObject.TrackProperty(
            //    p => p.Name,
            //    e.OldValue,
            //    e.NewValue,
            //    Resource.GameObjectRenameStringFormat.FormatWith(e.OldValue, e.NewValue)
            //);
        }
    }
}