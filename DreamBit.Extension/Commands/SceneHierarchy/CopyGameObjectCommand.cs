using DreamBit.Extension.Models;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface ICopyGameObjectCommand : ICommand
    {
        void Execute(ISceneObject sceneObject);
    }

    internal sealed class CopyGameObjectCommand : BaseCommand, ICopyGameObjectCommand
    {
        public void Execute(ISceneObject sceneObject)
        {
        }
    }
}