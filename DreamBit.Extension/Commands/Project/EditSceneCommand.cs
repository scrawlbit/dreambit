using DreamBit.Extension.Components;
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
        private IHierarchyBridge _hierarchy;
        private SceneFile _font;

        public EditSceneCommand(IPackageBridge package, IProject project)
        {
            _package = package;
            _project = project;
        }

        protected override int Id => DreamBitPackage.Guids.EditSceneCommand;

        public override void Execute(object parameter)
        {
            // TODO open scene in MonoGame window
        }
        protected override bool CanShow(object parameter)
        {
            _font = null;

            if (_package.IsSingleHierarchySelected(out _hierarchy))
                _font = _project.Files.GetByPath(_hierarchy.Path) as SceneFile;

            return _font != null;
        }
    }
}
