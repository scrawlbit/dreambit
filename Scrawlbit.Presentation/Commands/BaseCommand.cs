using System;
using System.Linq;
using System.Windows.Input;

namespace Scrawlbit.Presentation.Commands
{
    public class BaseCommand : ICommand
    {
        public static readonly object NoParams = new object();
        private readonly CommandMethod[] _canExecuteMethods;
        private readonly CommandMethod[] _executeMethods;

        public BaseCommand()
        {
            _canExecuteMethods = GetMethods("CanExecute");
            _executeMethods = GetMethods("Execute");
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        bool ICommand.CanExecute(object value)
        {
            if (!_canExecuteMethods.Any())
                return true;

            CommandMethod method = _canExecuteMethods.Single(m => m.CanExecute(value));

            return (bool)method.Execute(this, value);
        }
        void ICommand.Execute(object value)
        {
            CommandMethod method = _executeMethods.Single(m => m.CanExecute(value));

            method.Execute(this, value);
        }

        private CommandMethod[] GetMethods(string name)
        {
            return GetType()
                .GetMethods()
                .Where(i => i.Name == name)
                .Select(i => new CommandMethod(i))
                .OrderBy(i => !i.HasParameters)
                .ToArray();
        }
    }
}