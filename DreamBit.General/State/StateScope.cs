using System;

namespace DreamBit.General.State
{
    public interface IStateScope : IDisposable
    {
    }

    internal class StateScope : IStateScope
    {
        private readonly IStateTransaction _transaction;

        public StateScope(IStateTransaction transaction = null)
        {
            _transaction = transaction;
        }

        public void Dispose()
        {
            _transaction?.End();
        }
    }
}
