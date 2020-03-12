using System;

namespace DreamBit.General.State
{
    public interface IStateCommand : IStateUnit
    {
        void Do();
        void Undo();
    }

    public class StateCommand : IStateCommand
    {
        public string Description { get; set; }
        public Action Do { get; set; }
        public Action Undo { get; set; }

        void IStateCommand.Do() => Do();
        void IStateCommand.Undo() => Undo();
    }
}
