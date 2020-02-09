using DreamBit.Extension.Models;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    public interface IPasteGameObjectCommand : ICommand
    {
        void Execute(ISceneObject sceneObject);
    }
    public sealed class PasteGameObjectCommand : BaseCommand, IPasteGameObjectCommand
    {
        public void Execute(ISceneObject sceneObject)
        {
        }
    }
}