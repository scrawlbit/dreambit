using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using Scrawlbit.Presentation.Commands;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneInspect
{
    public interface IRemoveGameComponentCommand : ICommand
    {
        bool CanExecute(GameComponent component);
        void Execute(GameComponent component);
    }
    internal class RemoveGameComponentCommand : BaseCommand, IRemoveGameComponentCommand
    {
        private readonly IEditor _editor;

        public RemoveGameComponentCommand(IEditor editor)
        {
            _editor = editor;
        }

        public bool CanExecute(GameComponent component)
        {
            return _editor.SelectedObject?.Components.Contains(component) == true;
        }
        public void Execute(GameComponent component)
        {
            _editor.SelectedObject.Components.Remove(component);
        }
    }
}
