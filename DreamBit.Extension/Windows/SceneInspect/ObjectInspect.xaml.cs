using DreamBit.Extension.Module;
using System.Windows;

namespace DreamBit.Extension.Windows.SceneInspect
{
    public partial class ObjectInspect
    {
        public ObjectInspect()
        {
            InitializeComponent();
        }

        private ISelectionObject Selection => (ISelectionObject)Panel.DataContext;

        private void StartEditing(object sender, RoutedEventArgs e)
        {
            Selection.InEdition = true;
        }
        private void StopEditing(object sender, RoutedEventArgs e)
        {
            Selection.InEdition = false;
        }
    }
}
