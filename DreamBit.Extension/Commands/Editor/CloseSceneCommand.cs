using DreamBit.Extension.Components;
using DreamBit.Extension.Management;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface ICloseSceneCommand : IToolCommand { }
    internal class CloseSceneCommand : ToolCommand, ICloseSceneCommand
    {
        private readonly IEditor _editor;

        public CloseSceneCommand(IEditor editor)
        {
            _editor = editor;
        }

        protected override int Id => DreamBitPackage.Guids.CloseSceneCommand;

        public override bool CanExecute()
        {
            return _editor.OpenedSceneFile != null;
        }
        public override void Execute()
        {
            _editor.OpenedSceneFile = null;
        }
    }
}
