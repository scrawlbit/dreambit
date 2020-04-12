using DreamBit.Extension.ViewModels;
using Scrawlbit.Presentation.Dependency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DreamBit.Extension.Windows.SceneInspect
{
    public class SceneInspectPartial : UserControl
    {
        private static readonly DependencyProperty<SceneInspectPartial, SceneInspectViewModel> ViewModelProperty;

        static SceneInspectPartial()
        {
            var registry = new DependencyRegistry<SceneInspectPartial>();

            ViewModelProperty = registry.Property(s => s.ViewModel);
        }
        public SceneInspectPartial()
        {
            BindingOperations.SetBinding(this, ViewModelProperty, new Binding
            {
                Path = new PropertyPath("DataContext"),
                RelativeSource = new RelativeSource
                {
                    Mode = RelativeSourceMode.FindAncestor,
                    AncestorType = typeof(SceneInspectView)
                }
            });
        }

        public SceneInspectViewModel ViewModel
        {
            get => ViewModelProperty.Get(this);
            set => ViewModelProperty.Set(this, value);
        }
    }
}
