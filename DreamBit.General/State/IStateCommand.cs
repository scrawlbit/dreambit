namespace DreamBit.General.State
{
    public interface IStateCommand : IStateUnit
    {
        void Do();
        void Undo();
    }
}
