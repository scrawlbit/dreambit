using Scrawlbit.Presentation.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace Scrawlbit.Presentation.Helpers
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
    }
}