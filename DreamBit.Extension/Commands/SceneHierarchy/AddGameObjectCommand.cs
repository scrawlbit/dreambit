using DreamBit.Extension.Components;
using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.General.State;
using Scrawlbit.Collections;
using System.Linq;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IAddGameObjectCommand : IToolCommand
    {
        bool CanExecute();
        void Execute();
        void Execute(GameObject gameObject);
    }
    internal sealed class AddGameObjectCommand : ToolCommand, IAddGameObjectCommand
    {
        private readonly IEditor _editor;
        private readonly IStateManager _state;

        public AddGameObjectCommand(IEditor editor, IStateManager state)
        {
            _editor = editor;
            _state = state;
        }

        protected override int Id => DreamBitPackage.Guids.AddGameObjectCommand;

        public override bool CanExecute()
        {
            return _editor.OpenedScene != null;
        }
        public override void Execute()
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
