using DreamBit.Extension.Module;
using DreamBit.General.State;
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

        private void OnIsVisibleChanged(object sender, ValueChangedEventArgs<bool?> e)
        {
            Selection.ValidateChanges();
        }
    }
}
