using System;

namespace Scrawlbit.Presentation.Commands
{
    public class DelegateCommand : BaseCommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute()
        {
            _execute();
        }
        public bool CanExecute()
        {
            if (_canExecute == null)
                return true;

            return _canExecute();
        }
    }

    public class DelegateCommand<T> : BaseCommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(T parameter)
        {
            _execute(parameter);
        }
        public bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
    }
}