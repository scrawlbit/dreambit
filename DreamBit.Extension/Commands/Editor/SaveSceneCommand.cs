using DreamBit.Extension.Components;
using DreamBit.Extension.Management;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface ISaveSceneCommand : IToolCommand { }
    internal class SaveSceneCommand : ToolCommand, ISaveSceneCommand
    {
        private readonly IEditor _editor;

        public SaveSceneCommand(IEditor editor)
        {
            _editor = editor;
        }

        protected override int Id => DreamBitPackage.Guids.SaveSceneCommand;

        public override bool CanExecute()
        {
            return _editor.OpenedSceneFile != null;
        }
        public override void Execute()
        {
            _editor.OpenedSceneFile.Save();
        }
    }
}
