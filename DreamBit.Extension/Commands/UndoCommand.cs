using DreamBit.Extension.Components;
using DreamBit.General.State;
using System.Linq;

namespace DreamBit.Extension.Commands
{
    internal interface IUndoCommand : IToolCommand
    {
        bool CanExecute();
        void Execute();
    }

    internal sealed class UndoCommand : ToolCommand, IUndoCommand
    {
        private readonly IStateManager _state;

        public UndoCommand(IStateManager state)
        {
            _state = state;
        }

        protected override int Id => DreamBitPackage.Guids.UndoCommand;

        public override bool CanExecute()
        {
            return _state.Undos.Any();
        }
        public override void Execute()
        {
            _state.Undo();
        }
    }
}
