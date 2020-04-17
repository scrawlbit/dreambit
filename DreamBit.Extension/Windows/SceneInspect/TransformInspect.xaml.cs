using DreamBit.Extension.Controls.Input;
using DreamBit.Extension.Module;
using DreamBit.General.State;

namespace DreamBit.Extension.Windows.SceneInspect
{
    public partial class TransformInspect
    {
        public TransformInspect()
        {
            InitializeComponent();
        }

        private void OnTransformChanged(FloatBox sender, ValueChangedEventArgs<float> e)
        {
            ((ISelectionObject)sender.DataContext).ValidateChanges();
        }
    }
}
