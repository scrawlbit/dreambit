using DreamBit.Extension.Components;
using DreamBit.General.State;
using System.Linq;

namespace DreamBit.Extension.Commands.Editor
{
    internal interface IRedoCommand : IToolCommand
    {
        bool CanExecute();
        void Execute();
    }

    internal sealed class RedoCommand : ToolCommand, IRedoCommand
    {
        private readonly IStateManager _state;

        public RedoCommand(IStateManager state)
        {
            _state = state;
            _state.Dos.CollectionChanged += (s, e) => QueryStatus();
        }

        protected override int Id => DreamBitPackage.Guids.RedoCommand;

        public override bool CanExecute()
        {
            return _state.Dos.Any();
        }
        public override void Execute()
        {
            _state.Do();
        }
    }
}
