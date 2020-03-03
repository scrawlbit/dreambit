using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    public interface IPasteGameObjectCommand : ICommand
    {
        bool CanExecute();
        void Execute(GameObject gameObject);
    }
    public sealed class PasteGameObjectCommand : BaseCommand, IPasteGameObjectCommand
    {
        private readonly IEditor _editor;

        public PasteGameObjectCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null;
        }
        public void Execute(GameObject gameObject)
        {
        }
    }
}