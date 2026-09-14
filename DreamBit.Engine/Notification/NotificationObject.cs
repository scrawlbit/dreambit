using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DreamBit.Engine.Notification
{
    /// <summary>
    /// Base bindável (INotifyPropertyChanged) para o modelo da engine.
    /// </summary>
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
