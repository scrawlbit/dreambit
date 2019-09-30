using System.ComponentModel;
using System.Runtime.CompilerServices;

// ReSharper disable ExplicitCallerInfoArgument
namespace Scrawlbit.Notification
{
    public class NotificationObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
        internal readonly NotificationComponent NotificationComponent;

        public NotificationObject()
        {
            NotificationComponent = new NotificationComponent(this);
        }

        public virtual event PropertyChangingEventHandler PropertyChanging
        {
            add => NotificationComponent.PropertyChanging += value;
            remove => NotificationComponent.PropertyChanging -= value;
        }
        public virtual event PropertyChangedEventHandler PropertyChanged
        {
            add => NotificationComponent.PropertyChanged += value;
            remove => NotificationComponent.PropertyChanged -= value;
        }

        protected virtual void OnPropertyChanging<T>(T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            NotificationComponent.OnPropertyChanging(oldValue, newValue, propertyName);
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            NotificationComponent.OnPropertyChanged(propertyName);
        }

        protected virtual bool Set<T>(ref T variable, T value, [CallerMemberName] string propertyName = null)
        {
            return NotificationComponent.Set(ref variable, value, propertyName);
        }
    }
}