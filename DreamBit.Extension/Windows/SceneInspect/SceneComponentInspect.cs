using DreamBit.Extension.Controls.Containers;
using DreamBit.Extension.Helpers;
using DreamBit.Extension.ViewModels;
using Scrawlbit.Presentation.Data;
using Scrawlbit.Presentation.Dependency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DreamBit.Extension.Windows.SceneInspect
{
    public class SceneComponentInspect : ExpansionPanel
    {
        public static readonly DependencyProperty<SceneComponentInspect, SceneInspectViewModel> ViewModelProperty;

        static SceneComponentInspect()
        {
            var registry = new DependencyRegistry<SceneComponentInspect>();

            ViewModelProperty = registry.Property(s => s.ViewModel);
        }
        public SceneComponentInspect()
        {
            if (this.IsInDesignMode())
                return;

            SetViewModelBinding();
            SetMenuDataContextBinding();
            SetStyle();
            CreateMenu();
        }

        public SceneInspectViewModel ViewModel
        {
            get => ViewModelProperty.Get(this);
            set => ViewModelProperty.Set(this, value);
        }

        private void SetViewModelBinding()
        {
            BindingOperations.SetBinding(this, ViewModelProperty, new Binding(nameof(DataContext))
            {
                RelativeSource = new RelativeSource
                {
                    Mode = RelativeSourceMode.FindAncestor,
                    AncestorType = typeof(SceneInspectView)
                }
            });
        }
        private void SetMenuDataContextBinding()
        {
            BindingOperations.SetBinding(this, MenuDataContextProperty, new Binding(nameof(ViewModel))
            {
                Source = this
            });
        }
        private void SetStyle()
        {
            Style = (Style)Application.Current.FindResource("ExpansionPanelStyle");
        }
        private void CreateMenu()
        {
            var menu = new ContextMenu();
            var item = new MenuItem();

            menu.Items.Add(item);
            item.Header = "Remove";

            BindingOperations.SetBinding(item, MenuItem.CommandProperty, new ContextMenuBinding(nameof(ViewModel.RemoveGameComponentCommand)));
            BindingOperations.SetBinding(item, MenuItem.CommandParameterProperty, new Binding());

            Menu = menu;
        }
    }
}
