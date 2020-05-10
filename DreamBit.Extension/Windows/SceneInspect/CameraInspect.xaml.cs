using DreamBit.Extension.Controls.Input;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.General.State;
using Newtonsoft.Json.Linq;
using Scrawlbit.Helpers;
using Scrawlbit.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Extension.Windows.SceneInspect
{
    public partial class CameraInspect
    {
        public CameraInspect()
        {
            InitializeComponent();
        }

        private Camera Camera
        {
            get => (Camera)DataContext;
        }

        private void OnActiveChanged(CheckBox sender, ValueChangedEventArgs<bool?> e)
        {
            var cameras = ViewModel.Editor.OpenedScene.GetCameras();

            Camera newCamera = Camera;
            Camera oldCamera = cameras.Except(newCamera).FirstOrDefault(c => c.IsActive);

            string description = $"{newCamera.GameObject.Name}'s camera set as active";
            IStateCommand newChange = newCamera.State().SetProperty(c => c.IsActive, false, true, description);
            IStateCommand oldChange = oldCamera?.State().SetProperty(c => c.IsActive, true, false, description);

            using (ViewModel.State.Scope("Active camera changed"))
            {
                ViewModel.State.Add(newChange);
                ViewModel.State.Execute(oldChange);
            }
        }
        private void OnTargetChanged(GameObjectSelector sender, ValueChangedEventArgs<Guid> e)
        {
            GameObject target = ViewModel.Editor.OpenedScene.Objects.Find(e.NewValue);
            string description = $"{Camera.GameObject.Name}'s camera target set to \"{target?.Name}\"";
            IStateCommand command = Camera.State().SetProperty(c => c.Target, e, description);

            ViewModel.State.Add(command);
        }
    }
}
