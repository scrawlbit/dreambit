using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IRemoveGameObjectCommand : ICommand
    {
        void Execute(GameObject gameObject);
    }

    internal sealed class RemoveGameObjectCommand : BaseCommand, IRemoveGameObjectCommand
    {
        private readonly IEditor _editor;

        public RemoveGameObjectCommand(IEditor editor)
        {
            _editor = editor;
        }

        public void Execute(GameObject gameObject)
        {
            var target = gameObject.Parent?.Children ?? _editor.OpenedScene.Objects;

            target.Remove(gameObject);
        }
    }
}