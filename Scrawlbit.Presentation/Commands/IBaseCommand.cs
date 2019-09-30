using System.Windows.Input;

namespace Scrawlbit.Presentation.Commands
{
    public interface IBaseCommand : ICommand
    {
        bool CanExecute();
        void Execute();
    }

    public interface IBaseCommand<in T> : ICommand
    {
        bool CanExecute(T parameter);
        void Execute(T parameter);
    }

    public interface IBaseCommand<in T1, in T2> : ICommand
    {
        bool CanExecute(T1 p1, T2 p2);
        void Execute(T1 p1, T2 p2);
    }
}