using System;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Scrawlbit.Notification.Notificator
{
    internal class NotificationConstruction
    {
        private readonly Notificator _notificator;
        private readonly NotificationConstructor _notificationConstructor;

        public NotificationConstruction(Notificator notificator, NotificationConstructor notificationConstructor)
        {
            _notificator = notificator;
            _notificationConstructor = notificationConstructor;
        }

        public NotificationConstructor On(PropertyInfo[] chainedProperties)
        {
            return _notificator.On(chainedProperties);
        }
        public NotificationConstructor Or()
        {
            return _notificationConstructor;
        }

        public static implicit operator Notificator(NotificationConstruction notificationConstruction)
        {
            return notificationConstruction._notificator;
        }
    }

    public class NotificationConstruction<T, TProperty>
    {
        private readonly Notificator<T> _notificator;
        private readonly NotificationConstructor<T, TProperty> _notificationConstructor;

        internal NotificationConstruction(Notificator<T> notificator, NotificationConstructor<T, TProperty> notificationConstructor)
        {
            _notificator = notificator;
            _notificationConstructor = notificationConstructor;
        }

        public NotificationConstructor<T, TNewProperty> On<TNewProperty>(Expression<Func<T, TNewProperty>> expression)
        {
            return _notificator.On(expression);
        }
        public NotificationConstructor<T, TProperty> Or()
        {
            return _notificationConstructor;
        }

        public static implicit operator Notificator<T>(NotificationConstruction<T, TProperty> notificationConstrution)
        {
            return notificationConstrution._notificator;
        }
    }
}