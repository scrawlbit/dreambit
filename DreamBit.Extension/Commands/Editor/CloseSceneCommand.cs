using DreamBit.Extension.Management;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface ICloseSceneCommand : ICommand { }
    internal class CloseSceneCommand : BaseCommand, ICloseSceneCommand
    {
        private readonly IEditor _editor;

        public CloseSceneCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute()
        {
            return _editor.OpenedSceneFile != null;
        }
        public void Execute()
        {
            _editor.OpenedSceneFile = null;
        }
    }
}
