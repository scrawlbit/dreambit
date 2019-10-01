using System.IO;
using System.Windows.Input;
using DreamBit.Extension.Components;
using DreamBit.Extension.Windows.Dialogs;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DreamBit.Extension.Commands.Project
{
    internal interface IAddFontCommand : ICommand { }
    internal sealed class AddFontCommand : ToolCommand, IAddFontCommand
    {
        private readonly IPackageBridge _package;

        public AddFontCommand(IPackageBridge package)
        {
            _package = package;
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
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_package.IsSingleHierarchySelected(out var hierarchy))
            {
                ((IVsProject)hierarchy).GetMkDocument(VSConstants.VSITEMID_ROOT, out var fileName);
                var directory = Path.GetDirectoryName(fileName);
                var dreamPath = Path.Combine(directory, "Game.dream");

                return File.Exists(dreamPath);
            }

            return false;
        }
    }
}