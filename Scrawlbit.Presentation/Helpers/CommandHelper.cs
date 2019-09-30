using ScrawlBit.Presentation.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace ScrawlBit.Presentation.Helpers
{
    public static class CommandHelper
    {
        internal static bool IsParameterValid(object parameter, bool allowNullValues)
        {
            return parameter != DependencyProperty.UnsetValue && (allowNullValues || parameter != null);
        }

        public static void TryExecute(this ICommand command, object value)
        {
            if (command?.CanExecute(value) == true)
                command.Execute(value);
        }
        public static void TryExecute<T>(this IBaseCommand<T> command, T value)
        {
            command.TryExecute((object)value);
        }
        public static void TryExecute<T1, T2>(this IBaseCommand<T1, T2> command, T1 p1, T2 p2)
        {
            command.TryExecute(new object[] { p1, p2 });
        }

        public static OverrideCommand Override(this IBaseCommand command, Action<Action> execute = null, Func<Func<bool>, bool> canExecute = null)
        {
            return new OverrideCommand(command, execute, canExecute);
        }
        public static OverrideCommand<T> Override<T>(this IBaseCommand<T> command, Action<Action<T>, T> execute = null, Func<Func<T, bool>, T, bool> canExecute = null)
        {
            return new OverrideCommand<T>(command, execute, canExecute);
        }
    }
}