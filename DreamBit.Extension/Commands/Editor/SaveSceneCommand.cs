using DreamBit.Extension.Management;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface ISaveSceneCommand : ICommand { }
    internal class SaveSceneCommand : BaseCommand, ISaveSceneCommand
    {
        private readonly IEditor _editor;

        public SaveSceneCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute()
        {
            return _editor.OpenedSceneFile != null;
        }
        public void Execute()
        {
            _editor.OpenedSceneFile.Save();
        }
    }
}
