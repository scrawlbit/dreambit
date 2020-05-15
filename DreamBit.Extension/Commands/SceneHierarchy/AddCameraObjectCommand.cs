using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.General.State;
using Scrawlbit.Presentation.Commands;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IAddCameraObjectCommand : ICommand
    {
        bool CanExecute();
        void Execute();
    }

    internal sealed class AddCameraObjectCommand : BaseCommand, IAddCameraObjectCommand
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;

        public AddCameraObjectCommand(IEditor editor, IStateManager state)
        {
            _editor = editor;
            _state = state;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null;
        }
        public void Execute()
        {
            GameObject gameObject = new GameObject();
            Camera camera = new Camera();

            camera.IsActive = !HasActiveCamera();
            gameObject.Name = GetNewName();
            gameObject.Components.Add(camera);

            IStateCommand command = _editor.OpenedScene.Objects.State().Add(gameObject, "Camera added to scene");

            _state.Execute(command);
            _editor.SelectObjects(gameObject);
        }

        private bool HasActiveCamera()
        {
            var objects = _editor.OpenedScene.Objects;
            var components = objects.SelectMany(g => g.Components);
            var cameras = components.OfType<Camera>();

            return cameras.Any(c => c.IsActive);
        }
        private string GetNewName()
        {
            string name = "Camera";
            string finalName = name;
            int i = 0;

            while (_editor.OpenedScene.Objects.Any(o => o.Name == finalName))
                finalName = string.Format("{0} {1}", name, ++i);

            return finalName;
        }
    }
}