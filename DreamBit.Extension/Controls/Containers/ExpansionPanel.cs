using Scrawlbit.Presentation.Converters;
using Scrawlbit.Presentation.Dependency;
using Scrawlbit.Presentation.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DreamBit.Extension.Controls.Containers
{
    public class ExpansionPanel : ContentControl
    {
        public static readonly DependencyProperty<ExpansionPanel, string> HeaderProperty;
        public static readonly DependencyProperty<ExpansionPanel, ContextMenu> MenuProperty;
        public static readonly DependencyProperty<ExpansionPanel, object> MenuDataContextProperty;
        public static readonly DependencyProperty<ExpansionPanel, bool> HighlightProperty;

        static ExpansionPanel()
        {
            var dependency = new DependencyRegistry<ExpansionPanel>();

            HeaderProperty = dependency.Property(i => i.Header);
            MenuProperty = dependency.Property(i => i.Menu);
            MenuDataContextProperty = dependency.Property(i => i.MenuDataContext);
            HighlightProperty = dependency.Property(i => i.Highlight);
        }
        public ExpansionPanel()
        {
            if (ApplicationHelper.IsInDesignMode())
                return;

            BindingOperations.SetBinding(this, HighlightProperty, new MultiBinding
            {
                Converter = new OrConverter(),
                Bindings =
                {
                    BindingHelper.SourcePath(this, i => i.IsKeyboardFocusWithin),
                    BindingHelper.SourcePath(this, i => i.Menu.IsOpen)
                }
            });
        }

        public string Header
        {
            get { return HeaderProperty.Get(this); }
            set { HeaderProperty.Set(this, value); }
        }
        public ContextMenu Menu
        {
            get { return MenuProperty.Get(this); }
            set { MenuProperty.Set(this, value); }
        }
        public object MenuDataContext
        {
            get { return MenuDataContextProperty.Get(this); }
            set { MenuDataContextProperty.Set(this, value); }
        }
        public bool Highlight
        {
            get { return HighlightProperty.Get(this); }
            set { HighlightProperty.Set(this, value); }
        }
    }
}
