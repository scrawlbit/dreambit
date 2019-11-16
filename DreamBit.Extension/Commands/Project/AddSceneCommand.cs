using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Extension.Windows.Dialogs;
using DreamBit.Game.Files;
using DreamBit.Project;

namespace DreamBit.Extension.Commands.Project
{
    internal interface IAddSceneCommand : IToolCommand { }
    internal class AddSceneCommand : ToolCommand, IAddSceneCommand
    {
        private readonly IPackageBridge _package;
        private readonly IProjectManager _manager;
        private readonly IProject _project;
        private IHierarchyBridge _hierarchy;

        public AddSceneCommand(IPackageBridge package, IProjectManager manager, IProject project)
        {
            _package = package;
            _manager = manager;
            _project = project;
        }

        protected override int Id => DreamBitPackage.Guids.AddSceneCommand;

        public override void Execute(object parameter)
        {
            var dialog = new FileNameDialog();

            dialog.FileNameInformed += OnFileNameInformed;
            dialog.Open("New Scene");
        }
        protected override bool CanShow(object parameter)
        {
            return _manager.IsSingleHierarchySelected(out _hierarchy);
        }

        private void OnFileNameInformed(string name)
        {
            _manager.AddFileOnSelectedPath<SceneFile>(_hierarchy, name);
        }
    }
}
