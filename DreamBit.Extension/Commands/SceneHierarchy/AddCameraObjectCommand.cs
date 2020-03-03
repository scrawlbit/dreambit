using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IAddCameraObjectCommand : IToolCommand
    {
        bool CanExecute();
        void Execute();
        void Execute(GameObject gameObject);
    }
    internal sealed class AddCameraObjectCommand : ToolCommand, IAddCameraObjectCommand
    {
        private readonly IEditor _editor;

        public AddCameraObjectCommand(IEditor editor)
        {
            _editor = editor;
        }

        protected override int Id => DreamBitPackage.Guids.AddCameraObjectCommand;

        public override bool CanExecute()
        {
            return _editor.OpenedScene != null;
        }
        public override void Execute()
        {
        }
        public void Execute(GameObject gameObject)
        {
        }
    }
}