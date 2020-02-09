using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Extension.Windows.Dialogs;

namespace DreamBit.Extension.Commands.Project
{
    internal interface IAddFontCommand : IToolCommand { }
    internal sealed class AddFontCommand : ToolCommand, IAddFontCommand
    {
        private readonly IProjectManager _manager;
        private IHierarchyBridge _hierarchy;

        public AddFontCommand(IProjectManager manager)
        {
            _manager = manager;
        }

        protected override int Id => DreamBitPackage.Guids.AddFontCommand;

        public override void Execute()
        {
            var dialog = new EditFontDialog();

            dialog.NewFont(_hierarchy);
        }
        protected override bool CanShow()
        {
            return _manager.IsSingleHierarchySelected(out _hierarchy);
        }
    }
}