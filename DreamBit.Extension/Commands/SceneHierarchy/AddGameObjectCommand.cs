using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
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

        public AddGameObjectCommand(IEditor editor)
        {
            _editor = editor;
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
            Add(gameObject.Children);

            gameObject.IsExpanded = true;
        }

        private void Add(IGameObjectCollection collection)
        {
            GameObject[] objects = collection.ToArray();

            string name = "Game Object";
            string finalName = name;
            int i = 0;

            while (objects.Any(o => o.Name == finalName))
                finalName = string.Format("{0} {1}", name, ++i);

            collection.Add(new GameObject
            {
                Name = finalName,
                IsSelected = true
            });
        }
    }
}
