using DreamBit.General.State;
using Scrawlbit.Presentation.Commands;
using System.Linq;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface IRedoCommand : ICommand
    {
        bool CanExecute();
        void Execute();
    }

    internal sealed class RedoCommand : BaseCommand, IRedoCommand
    {
        private readonly IStateManager _state;

        public RedoCommand(IStateManager state)
        {
            _state = state;
        }

        public bool CanExecute()
        {
            return _state.Dos.Any();
        }
        public void Execute()
        {
            _state.Do();
        }
    }
}
