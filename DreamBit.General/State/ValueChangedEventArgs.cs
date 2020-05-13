using System.Windows;

namespace DreamBit.General.State
{
    public class ValueChangedEventArgs<T> : RoutedEventArgs
    {
        public ValueChangedEventArgs()
        {
        }
        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; set; }
        public T NewValue { get; set; }

        public static implicit operator ValueChangedEventArgs<T>((T OldValue, T NewValue) e)
        {
            return new ValueChangedEventArgs<T>(e.OldValue, e.NewValue);
        }
    }
}
