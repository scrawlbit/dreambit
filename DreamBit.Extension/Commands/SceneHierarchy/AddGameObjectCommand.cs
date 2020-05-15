using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.General.State;
using Scrawlbit.Presentation.Commands;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IAddGameObjectCommand : ICommand
    {
        bool CanExecute();
        void Execute();
        void Execute(GameObject gameObject);
    }
    internal sealed class AddGameObjectCommand : BaseCommand, IAddGameObjectCommand
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;

        public AddGameObjectCommand(IEditor editor, IStateManager state)
        {
            _editor = editor;
            _state = state;
        }

        public bool CanExecute()
        {
            return _editor.OpenedScene != null;
        }
        public void Execute()
        {
            Add(_editor.OpenedScene.Objects);
        }
        public void Execute(GameObject gameObject)
        {
            Add(gameObject.Children, gameObject);

            gameObject.IsExpanded = true;
        }

        private void Add(IGameObjectCollection collection, GameObject parent = null)
        {
            string name = GetNewName(collection);
            string parentName = parent?.Name ?? "Scene";
            string description = $"{name} added to {parentName}";
            
            GameObject gameObject = new GameObject { Name = name };
            IStateCommand command = collection.State().Add(gameObject, description);

            _state.Execute(command);
            _editor.SelectObjects(gameObject);
        }

        private static string GetNewName(IGameObjectCollection collection)
        {
            GameObject[] objects = collection.ToArray();

            string name = "Game Object";
            string finalName = name;
            int i = 0;

            while (objects.Any(o => o.Name == finalName))
                finalName = string.Format("{0} {1}", name, ++i);

            return finalName;
        }
    }
}
