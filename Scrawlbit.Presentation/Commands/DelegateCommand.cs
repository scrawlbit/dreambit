using System;

namespace ScrawlBit.Presentation.Commands
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

        public override void Execute()
        {
            _execute();
        }
        public override bool CanExecute()
        {
            if (_canExecute == null)
                return true;

            return _canExecute();
        }
    }

    public class DelegateCommand<T> : BaseCommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null, bool allowNullValues = false)
        {
            _execute = execute;
            _canExecute = canExecute;
            AllowNullValues = allowNullValues;
        }

        public override void Execute(T parameter)
        {
            _execute(parameter);
        }
        public override bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
    }
}