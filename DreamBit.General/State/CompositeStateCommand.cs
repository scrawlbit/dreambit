using System.Collections.Generic;
using System.Linq;

namespace DreamBit.General.State
{
    internal class CompositeStateCommand : IStateCommand
    {
        private readonly IStateCommand[] _commands;

        public CompositeStateCommand(IEnumerable<IStateCommand> commands, string description)
        {
            _commands = commands.ToArray();

            Description = description;
        }

        public string Description { get; }

        public void Do()
        {
            foreach (var state in _commands)
                state.Do();
        }
        public void Undo()
        {
            foreach (var state in _commands.Reverse())
                state.Undo();
        }
    }
}
