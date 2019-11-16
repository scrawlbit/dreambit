using DreamBit.Extension.Components;
using DreamBit.Extension.Windows.Dialogs;
using DreamBit.Pipeline.Files;
using DreamBit.Project;
using DreamBit.Project.Helpers;

namespace DreamBit.Extension.Commands.Project
{
    internal interface IEditFontCommand : IToolCommand { }
    internal class EditFontCommand : ToolCommand, IEditFontCommand
    {
        private readonly IPackageBridge _package;
        private readonly IProject _project;
        private IHierarchyBridge _hierarchy;
        private PipelineFont _font;

        public EditFontCommand(IPackageBridge package, IProject project)
        {
            _package = package;
            _project = project;
        }

        protected override int Id => DreamBitPackage.Guids.EditFontCommand;

        public override void Execute(object parameter)
        {
            new EditFontDialog().EditFont(_font);
        }
        protected override bool CanShow(object parameter)
        {
            if (!_package.IsSingleHierarchySelected(out _hierarchy))
                return false;

            _font = _project.Files.GetByPath(_hierarchy.Path) as PipelineFont;

            return _font != null;
        }
    }
}
