using DreamBit.Extension.Components;
using DreamBit.Extension.Models;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IAddGameObjectCommand : IToolCommand
    {
        bool CanExecute(ISceneObject sceneObject);

        void Execute();
        void Execute(ISceneObject sceneObject);
    }
    internal sealed class AddGameObjectCommand : ToolCommand, IAddGameObjectCommand
    {
        private readonly IEditingScene _scene;

        public AddGameObjectCommand(IEditingScene scene)
        {
            _scene = scene;
        }

        protected override int Id => DreamBitPackage.Guids.AddGameObjectCommand;

        public bool CanExecute(ISceneObject sceneObject)
        {
            return sceneObject != null;
        }

        public override void Execute()
        {
            Add(_scene.Objects);
        }
        public void Execute(ISceneObject sceneObject)
        {
            Add(sceneObject.Children);
        }

        private void Add(ISceneObjectCollection collection)
        {
            collection.Add("Game Object");
        }
    }
}
