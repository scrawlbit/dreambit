using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Game.Files;
using DreamBit.Project;
using DreamBit.Project.Helpers;

namespace DreamBit.Extension.Commands.Project
{
    internal interface IEditSceneCommand : IToolCommand { }
    internal class EditSceneCommand : ToolCommand, IEditSceneCommand
    {
        private readonly IPackageBridge _package;
        private readonly IProject _project;
        private readonly IEditor _editor;
        private IHierarchyBridge _hierarchy;
        private SceneFile _scene;

        public EditSceneCommand(IPackageBridge package, IProject project, IEditor editor)
        {
            _package = package;
            _project = project;
            _editor = editor;
        }

        protected override int Id => DreamBitPackage.Guids.EditSceneCommand;

        public override void Execute()
        {
            _editor.OpenedSceneFile = _scene;
            _editor.OpenedScene = _scene.Scene;
        }
        protected override bool CanShow()
        {
            _scene = null;

            if (_package.IsSingleHierarchySelected(out _hierarchy))
                _scene = _project.Files.GetByPath(_hierarchy.Path) as SceneFile;

            return _scene != null;
        }
    }
}
