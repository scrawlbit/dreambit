using DreamBit.Game.Elements;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface ICopyGameObjectCommand : ICommand
    {
        void Execute(GameObject gameObject);
    }

    internal sealed class CopyGameObjectCommand : BaseCommand, ICopyGameObjectCommand
    {
        public void Execute(GameObject gameObject)
        {
        }
    }
}