using DreamBit.Extension.Controls.Input;
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

        private void OnIsVisibleChanged(CheckBox sender, ValueChangedEventArgs<bool?> e)
        {
            ((ISelectionObject)sender.DataContext).ValidateChanges();
        }
    }
}
