using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Extension.Windows.Dialogs;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Project
{
    internal interface IAddFontCommand : ICommand { }
    internal sealed class AddFontCommand : ToolCommand, IAddFontCommand
    {
        private readonly IPackageBridge _package;
        private readonly IProjectManager _manager;

        public AddFontCommand(IPackageBridge package, IProjectManager manager)
        {
            _package = package;
            _manager = manager;
        }

        protected override int Id => DreamBitPackage.Guids.AddFontCommand;

        public override void Execute(object parameter)
        {
            var folder = _package.GetSelectedItemPath();
            var dialog = new EditFontDialog();

            dialog.NewFont(folder);
        }
        protected override bool CanShow(object parameter)
        {
            return _manager.IsSingleItemSelected();
        }
    }
}