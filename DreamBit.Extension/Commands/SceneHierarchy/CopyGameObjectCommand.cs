using DreamBit.Extension.Models;
using Scrawlbit.Presentation.Commands;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface ICopyGameObjectCommand : IBaseCommand<ISceneObject> { }
    internal sealed class CopyGameObjectCommand : BaseCommand<ISceneObject>, ICopyGameObjectCommand
    {
        public override void Execute(ISceneObject sceneObject)
        {
        }
    }
}