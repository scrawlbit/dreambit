using DreamBit.Extension.Models;
using Scrawlbit.Presentation.Commands;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    public interface IPasteGameObjectCommand : IBaseCommand<ISceneObject> { }
    public sealed class PasteGameObjectCommand : BaseCommand<ISceneObject>, IPasteGameObjectCommand
    {
        public PasteGameObjectCommand()
        {
            AllowNullValues = true;
        }

        public override void Execute(ISceneObject sceneObject)
        {
        }
    }
}