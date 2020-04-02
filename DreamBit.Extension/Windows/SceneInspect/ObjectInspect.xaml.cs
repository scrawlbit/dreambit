using DreamBit.Extension.ViewModels;
using DreamBit.Game.Elements;
using DreamBit.General.State;

namespace DreamBit.Extension.Windows.SceneInspect
{
    public partial class ObjectInspect
    {
        public ObjectInspect()
        {
            InitializeComponent();
        }

        private void OnIsVisibleChanged(object sender, bool oldValue, bool newValue)
        {
            var viewModel = (SceneInspectViewModel)DataContext;
            GameObject obj = viewModel.Editor.SelectedObject;

            string description = $"{obj.Name} changed to {(newValue ? "" : "not ")}visible";
            IStateCommand command = obj.State().SetProperty(g => g.IsVisible, oldValue, newValue, description);

            viewModel.State.Add(command);
        }
    }
}
