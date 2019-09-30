// ReSharper disable once CheckNamespace
namespace ScrawlBit.Notification.Notificator
{
    public delegate void OnPropertyChanging<in T>(T oldValue, T newValue);
    public delegate void OnPropertyChanged<in T>(T oldValue, T newValue);
}