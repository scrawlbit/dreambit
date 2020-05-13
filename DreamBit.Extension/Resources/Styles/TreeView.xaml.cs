using DreamBit.General.State;
using Microsoft.VisualStudio.PlatformUI;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Helpers;
using System.Windows;
using System.Windows.Controls;
using ControlHelper = DreamBit.Extension.Helpers.ControlHelper;

namespace DreamBit.Extension.Resources.Styles
{
    public partial class TreeViewStyle
    {
        private void OnPanelLoaded(object sender, RoutedEventArgs args)
        {
            StackPanel panel = (StackPanel)sender;
            MultiSelectTreeViewItem item = panel.ParentsUntil<MultiSelectTreeViewItem>();
            string oldValue = null;

            item.AddPropertyChangeHandler(MultiSelectTreeViewItem.IsEditingProperty, (s, e) =>
            {
                if (!item.IsEditable)
                    return;

                if (item.IsEditing)
                {
                    oldValue = item.DisplayName;
                    return;
                }

                string newValue = item.DisplayName.Trim().IfEmptyThenNull();

                if (newValue != oldValue)
                {
                    item.RaiseEvent(new ValueChangedEventArgs<string>
                    {
                        OldValue = oldValue,
                        NewValue = newValue,
                        RoutedEvent = ControlHelper.TextChangedEvent
                    });
                }
            });

            panel.Loaded -= OnPanelLoaded;
        }
    }
}
