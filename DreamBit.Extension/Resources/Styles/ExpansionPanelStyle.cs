using System.Windows;
using System.Windows.Controls;

namespace DreamBit.Extension.Resources.Styles.SceneInspect
{
    public partial class ExpansionPanelStyle
    {
        private void OnMenuButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }
}
