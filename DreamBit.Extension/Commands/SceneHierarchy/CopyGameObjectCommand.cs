using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.Game.Serialization;
using Scrawlbit.Presentation.Commands;
using System.Windows;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface ICopyGameObjectCommand : ICommand
    {
        bool CanExecute();
        bool CanExecute(GameObject gameObject);

        void Execute();
        void Execute(GameObject gameObject);
    }

    internal sealed class CopyGameObjectCommand : BaseCommand, ICopyGameObjectCommand
    {
        private readonly IEditor _editor;
        private readonly IGameElementsParser _parser;

        public CopyGameObjectCommand(IEditor editor, IGameElementsParser parser)
        {
            _editor = editor;
            _parser = parser;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null && _editor.SelectedObjects.Count == 1;
        }
        public bool CanExecute(GameObject gameObject)
        {
            return _editor.OpenedScene != null;
        }

        public void Execute()
        {
            Execute(_editor.SelectedObject);
        }
        public void Execute(GameObject gameObject)
        {
            var json = _parser.ToJson(gameObject);

            Clipboard.SetText(json);
        }
    }
}