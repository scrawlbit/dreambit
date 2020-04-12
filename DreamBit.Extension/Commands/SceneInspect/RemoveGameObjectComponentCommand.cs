using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using Scrawlbit.Presentation.Commands;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneInspect
{
    public interface IRemoveGameObjectComponentCommand : ICommand
    {
        bool CanExecute(GameObjectComponent component);
        void Execute(GameObjectComponent component);
    }
    internal class RemoveGameObjectComponentCommand : BaseCommand, IRemoveGameObjectComponentCommand
    {
        private readonly IEditor _editor;

        public RemoveGameObjectComponentCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute(GameObjectComponent component)
        {
            return _editor.SelectedObject?.Components.Contains(component) == true;
        }
        public void Execute(GameObjectComponent component)
        {
            _editor.SelectedObject.Components.Remove(component);
        }
    }
}
