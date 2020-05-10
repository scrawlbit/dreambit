using DreamBit.General.State;
using Scrawlbit.Presentation.Commands;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface IUndoCommand : ICommand
    {
        bool CanExecute();
        void Execute();
    }

    internal sealed class UndoCommand : BaseCommand, IUndoCommand
    {
        private readonly IStateManager _state;

        public UndoCommand(IStateManager state)
        {
            _state = state;
        }

        public bool CanExecute()
        {
            return _state.Undos.Any();
        }
        public void Execute()
        {
            _state.Undo();
        }
    }
}
