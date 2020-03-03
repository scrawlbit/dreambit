using DreamBit.Game.Elements;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    public interface IPasteGameObjectCommand : ICommand
    {
        void Execute(GameObject gameObject);
    }
    public sealed class PasteGameObjectCommand : BaseCommand, IPasteGameObjectCommand
    {
        public void Execute(GameObject gameObject)
        {
        }
    }
}