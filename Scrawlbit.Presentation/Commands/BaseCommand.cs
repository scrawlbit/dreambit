using System;
using System.Linq;
using System.Windows.Input;
using ScrawlBit.Notification;
using static ScrawlBit.Presentation.Commands.CommandHelper;

namespace ScrawlBit.Presentation.Commands
{
    // ReSharper disable once InconsistentNaming
    public abstract class _BaseCommand : NotificationObject
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public abstract class BaseCommand : _BaseCommand, IBaseCommand
    {
        public bool CanExecute(object parameter)
        {
            return CanExecute();
        }
        public void Execute(object parameter)
        {
            Execute();
        }

        public virtual bool CanExecute()
        {
            return true;
        }
        public abstract void Execute();
    }

    public abstract class BaseCommand<T> : _BaseCommand, IBaseCommand<T>
    {
        protected bool AllowNullValues { get; set; }

        public bool CanExecute(object parameter)
        {
            return IsParameterValid(parameter, AllowNullValues) && CanExecute((T)parameter);
        }
        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }

        public virtual bool CanExecute(T p)
        {
            return true;
        }
        public abstract void Execute(T p);
    }

    public abstract class BaseCommand<T1, T2> : _BaseCommand, IBaseCommand<T1, T2>
    {
        protected bool AllowNullValues { get; set; }

        public bool CanExecute(object parameter)
        {
            var array = parameter as object[];
            return array != null &&
                   array.All(v => IsParameterValid(v, AllowNullValues)) &&
                   CanExecute((T1)array[0], (T2)array[1]);
        }
        public void Execute(object parameter)
        {
            var array = (object[])parameter;
            Execute((T1)array[0], (T2)array[1]);
        }

        public virtual bool CanExecute(T1 p1, T2 p2)
        {
            return true;
        }
        public abstract void Execute(T1 p1, T2 p2);
    }
}