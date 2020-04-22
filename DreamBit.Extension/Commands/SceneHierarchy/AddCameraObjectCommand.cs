using DreamBit.Extension.Components;
using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Drawing;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.General.State;
using System.Linq;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IAddCameraObjectCommand : IToolCommand
    {
        bool CanExecute();
        void Execute();
    }
    internal sealed class AddCameraObjectCommand : ToolCommand, IAddCameraObjectCommand
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;
        private readonly IContentDrawer _drawer;

        public AddCameraObjectCommand(IEditor editor, IStateManager state, IContentDrawer drawer)
        {
            _editor = editor;
            _state = state;
            _drawer = drawer;
        }

        protected override int Id => DreamBitPackage.Guids.AddCameraObjectCommand;

        public override bool CanExecute()
        {
            return _editor.OpenedScene != null;
        }
        public override void Execute()
        {
            GameObject gameObject = new GameObject();
            Camera camera = new Camera(_drawer);

            camera.IsActive = !HasActiveCamera();
            gameObject.Name = GetNewName();
            gameObject.Components.Add(camera);

            IStateCommand command = _editor.OpenedScene.Objects.State().Add(gameObject, "Camera added to scene");

            _state.Execute(command);
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