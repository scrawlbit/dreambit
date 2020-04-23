using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.General.State;
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
        private readonly IStateManager _state;

        public RemoveGameComponentCommand(IEditor editor, IStateManager state)
        {
            _editor = editor;
            _state = state;
        }

        public bool CanExecute(GameComponent component)
        {
            return _editor.SelectedObject?.Components.Contains(component) == true;
        }
        public void Execute(GameComponent component)
        {
            string description = $"{component.GameObject.Name}'s {component.Name} removed.";
            IStateCommand command = _editor.SelectedObject.Components.State().Remove(component, description);

            _state.Execute(command);
        }
    }
}
