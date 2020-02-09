using DreamBit.Extension.Models;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IRemoveGameObjectCommand : ICommand
    {
        void Execute(ISceneObject sceneObject);
    }

    internal sealed class RemoveGameObjectCommand : BaseCommand, IRemoveGameObjectCommand
    {
        private readonly IEditingScene _scene;

        public RemoveGameObjectCommand(IEditingScene scene)
        {
            _scene = scene;
        }

        public void Execute(ISceneObject sceneObject)
        {
            var target = sceneObject.Parent?.Children ?? _scene.Objects;

            target.Remove(sceneObject);
        }
    }
}