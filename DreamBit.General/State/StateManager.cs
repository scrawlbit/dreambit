using Scrawlbit.Collections;
using Scrawlbit.Helpers;
using System.Linq;

namespace DreamBit.General.State
{
    public interface IStateManager
    {
        IReadOnlyObservableCollection<IStateUnit> Dos { get; }
        IReadOnlyObservableCollection<IStateUnit> Undos { get; }

        void Execute(IStateCommand state);
        void Add(IStateCommand state);
        void Do(int levels = 1);
        void Undo(int levels = 1);

        IStateScope Scope(string description);
        void Reset();
    }

    internal class StateManager : IStateManager
    {
        private readonly ExtendedObservableCollection<IStateUnit> _dos;
        private readonly ExtendedObservableCollection<IStateUnit> _undos;
        private IStateTransaction _transaction;

        public StateManager()
        {
            _dos = new ExtendedObservableCollection<IStateUnit>();
            _undos = new ExtendedObservableCollection<IStateUnit>();
        }

        public IReadOnlyObservableCollection<IStateUnit> Dos => _dos;
        public IReadOnlyObservableCollection<IStateUnit> Undos => _undos;

        public void Execute(IStateCommand state)
        {
            state?.Do();
            Add(state);
        }
        public void Add(IStateCommand state)
        {
            if (state == null)
                return;

            if (_transaction != null)
            {
                _transaction.Add(state);
                return;
            }

            _undos.Add(state);
            _dos.Clear();
        }
        public void Do(int levels)
        {
            while (levels-- > 0 && _dos.Pop(out IStateUnit state))
            {
                ((IStateCommand)state).Do();
                _undos.Add(state);
            }
        }
        public void Undo(int levels)
        {
            while (levels-- > 0 && _undos.Pop(out IStateUnit state))
            {
                ((IStateCommand)state).Undo();
                _dos.Add(state);
            }
        }

        public IStateScope Scope(string description)
        {
            if (_transaction == null)
            {
                _transaction = new StateTransaction(this, description);
                _transaction.Ended += () => _transaction = null;

                return new StateScope(_transaction);
            }

            return new StateScope();
        }

        public void Reset()
        {
            _dos.Clear();
            _undos.Clear();
        }
    }
}
