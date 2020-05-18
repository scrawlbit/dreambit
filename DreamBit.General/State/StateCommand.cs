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
        public StateCommand(string description = null, Action @do = null, Action undo = null)
        {
            Description = description;
            Do = @do;
            Undo = undo;
        }

        public string Description { get; set; }
        public Action Do { get; set; }
        public Action Undo { get; set; }

        void IStateCommand.Do() => Do?.Invoke();
        void IStateCommand.Undo() => Undo?.Invoke();
    }
}
