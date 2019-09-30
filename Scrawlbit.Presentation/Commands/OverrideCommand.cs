using System;

namespace ScrawlBit.Presentation.Commands
{
    public class OverrideCommand : IBaseCommand
    {
        private readonly IBaseCommand _command;
        private readonly Action<Action> _execute;
        private readonly Func<Func<bool>, bool> _canExecute;

        public OverrideCommand(IBaseCommand command, Action<Action> execute = null, Func<Func<bool>, bool> canExecute = null)
        {
            _command = command;
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => _command.CanExecuteChanged += value;
            remove => _command.CanExecuteChanged -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(() => _command.CanExecute(parameter));

            return _command.CanExecute(parameter);
        }
        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute(() => _command.Execute(parameter));
            else
                _command.Execute(parameter);
        }

        public bool CanExecute()
        {
            if (_canExecute != null)
                return _canExecute(_command.CanExecute);

            return _command.CanExecute();
        }
        public void Execute()
        {
            if (_execute != null)
                _execute(_command.Execute);
            else
                _command.Execute();
        }
    }

    public class OverrideCommand<T> : IBaseCommand<T>
    {
        private readonly IBaseCommand<T> _command;
        private readonly Action<Action<T>, T> _execute;
        private readonly Func<Func<T, bool>, T, bool> _canExecute;

        public OverrideCommand(IBaseCommand<T> command, Action<Action<T>, T> execute = null, Func<Func<T, bool>, T, bool> canExecute = null)
        {
            _command = command;
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => _command.CanExecuteChanged += value;
            remove => _command.CanExecuteChanged -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(p => _command.CanExecute((object)p), (T)parameter);

            return _command.CanExecute(parameter);
        }
        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute(p => _command.Execute((object)p), (T)parameter);
            else
                _command.Execute(parameter);
        }

        public bool CanExecute(T parameter)
        {
            if (_canExecute != null)
                return _canExecute(p => _command.CanExecute(p), parameter);

            return _command.CanExecute(parameter);
        }
        public void Execute(T parameter)
        {
            if (_execute != null)
                _execute(p => _command.Execute(p), parameter);
            else
                _command.Execute(parameter);
        }
    }
}