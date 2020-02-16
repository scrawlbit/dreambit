using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Extension.Windows.Dialogs;
using DreamBit.Game.Files;

namespace DreamBit.Extension.Commands.Project
{
    internal interface IAddScriptCommand : IToolCommand { }
    internal class AddScriptCommand : ToolCommand, IAddScriptCommand
    {
        private readonly IProjectManager _manager;
        private IHierarchyBridge _hierarchy;

        public AddScriptCommand(IProjectManager manager)
        {
            _manager = manager;
        }

        protected override int Id => DreamBitPackage.Guids.AddScriptCommand;

        public override void Execute()
        {
            var dialog = new FileNameDialog();

            dialog.FileNameInformed += OnFileNameInformed;
            dialog.Open("New Scene");
        }
        protected override bool CanShow()
        {
            return _manager.IsSingleHierarchySelected(out _hierarchy);
        }

        private void OnFileNameInformed(string name)
        {
            _manager.AddFileOnSelectedPath<ScriptFile>(_hierarchy, name);
        }
    }
}
