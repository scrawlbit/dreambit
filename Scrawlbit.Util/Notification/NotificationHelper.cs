using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Scrawlbit.Helpers;
using Scrawlbit.Notification.Notificator;
using Scrawlbit.Notification.State;

namespace Scrawlbit.Notification
{
    public static class NotificationHelper
    {
        public static ChangesTracker<T> GetTracker<T>(Action changeNotified)
        {
            if (typeof(T).HasInterface<INotifyCollectionChanged>())
                return new CollectionChangesTracker<T>(changeNotified);

            if (typeof(T).HasInterface<INotifyPropertyChanged>())
                return new PropertyChangesTracker<T>(changeNotified);

            return new NullChangesTracker<T>();
        }

        public static void OnPropertyChanging<TProperty>(this NotificationObject obj, TProperty oldValue, TProperty newValue, [CallerMemberName] string propertyName = null)
        {
            obj.NotificationComponent.OnPropertyChanging(oldValue, newValue, propertyName);
        }
        public static void OnPropertyChanging<T, TProperty>(this T obj, TProperty oldValue, TProperty newValue, Expression<Func<T, TProperty>> property) where T : NotificationObject
        {
            obj.NotificationComponent.OnPropertyChanging(oldValue, newValue, property);
        }
        public static void OnPropertyChanged(this NotificationObject obj, [CallerMemberName] string propertyName = null)
        {
            obj.NotificationComponent.OnPropertyChanged(propertyName);
        }
        public static void OnPropertyChanged<T, TProperty>(this T obj, Expression<Func<T, TProperty>> property) where T : NotificationObject
        {
            obj.NotificationComponent.OnPropertyChanged(property);
        }

        public static Notificator<T> Notify<T>(this T obj)
        {
            return new Notificator<T>(obj);
        }

        public static void AddHandler(this INotifyPropertyChanging obj, PropertyChangingEventHandler handler)
        {
            obj.PropertyChanging += handler;
        }
        public static void AddHandlers(this INotifyPropertyChanging obj, IEnumerable<PropertyChangingEventHandler> handlers)
        {
            foreach (var handler in handlers)
                obj.PropertyChanging += handler;
        }
        public static void AddHandler(this INotifyPropertyChanged obj, PropertyChangedEventHandler handler)
        {
            obj.PropertyChanged += handler;
        }
        public static void AddHandlers(this INotifyPropertyChanged obj, IEnumerable<PropertyChangedEventHandler> handlers)
        {
            foreach (var handler in handlers)
                obj.PropertyChanged += handler;
        }

        public static void RemoveHandlers(this INotifyPropertyChanging obj, IEnumerable<PropertyChangingEventHandler> handlers)
        {
            foreach (var handler in handlers)
                obj.PropertyChanging -= handler;
        }
        public static void RemoveHandlers(this INotifyPropertyChanged obj, IEnumerable<PropertyChangedEventHandler> handlers)
        {
            foreach (var handler in handlers)
                obj.PropertyChanged -= handler;
        }
    }
}