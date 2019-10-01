using DreamBit.Extension.Models;
using Scrawlbit.Presentation.Commands;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IRemoveGameObjectCommand : IBaseCommand<ISceneObject> { }
    internal sealed class RemoveGameObjectCommand : BaseCommand<ISceneObject>, IRemoveGameObjectCommand
    {
        private readonly IEditingScene _scene;

        public RemoveGameObjectCommand(IEditingScene scene)
        {
            _scene = scene;
        }

        public override void Execute(ISceneObject sceneObject)
        {
            var target = sceneObject.Parent?.Children ?? _scene.Objects;

            target.Remove(sceneObject);
        }
    }
}