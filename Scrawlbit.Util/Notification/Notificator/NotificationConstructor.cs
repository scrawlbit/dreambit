using System;
using System.ComponentModel;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Scrawlbit.Notification.Notificator
{
    internal class NotificationConstructor
    {
        public NotificationConstructor(Notificator notificator, PropertyInfo property)
        {
            Notificator = notificator;
            Property = property;
        }

        public Notificator Notificator { get; }
        public PropertyInfo Property { get; set; }
        
        public virtual NotificationConstruction Changing(Action onPropertyChanging)
        {
            Notificator.AddHandler((object s, PropertyChangingEventArgs e) =>
            {
                if (e.PropertyName == Property.Name)
                    onPropertyChanging();
            });

            return Construction();
        }
        public virtual NotificationConstruction Changing<TProperty>(OnPropertyChanging<TProperty> onPropertyChanging)
        {
            Notificator.AddHandler((s, e) =>
            {
                if (e.PropertyName == Property.Name)
                {
                    var oldValue = (e as InternalPropertyChangingEventArgs)?.OldValue ?? default(TProperty);
                    var newValue = (e as InternalPropertyChangingEventArgs)?.NewValue ?? default(TProperty);

                    onPropertyChanging((TProperty)oldValue, (TProperty)newValue);
                }
            });

            return Construction();
        }
        public virtual NotificationConstruction Changed(Action onPropertyChanged)
        {
            Notificator.AddHandler((object s, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == Property.Name)
                    onPropertyChanged();
            });

            return Construction();
        }
        public virtual NotificationConstruction Changed<TProperty>(OnPropertyChanged<TProperty> onPropertyChanged)
        {
            Notificator.AddHandler((s, e) =>
            {
                if (e.PropertyName == Property.Name)
                {
                    var oldValue = (e as InternalPropertyChangedEventArgs)?.OldValue ?? default(TProperty);
                    var newValue = (e as InternalPropertyChangedEventArgs)?.NewValue ?? default(TProperty);

                    onPropertyChanged((TProperty)oldValue, (TProperty)newValue);
                }
            });

            return Construction();
        }

        protected virtual NotificationConstruction Construction()
        {
            return new NotificationConstruction(Notificator, this);
        }
    }

    public class NotificationConstructor<T, TProperty>
    {
        private readonly Notificator<T> _notificator;
        private readonly NotificationConstructor _notificationConstructor;

        internal NotificationConstructor(Notificator<T> notificator, NotificationConstructor notificationConstructor)
        {
            _notificator = notificator;
            _notificationConstructor = notificationConstructor;
        }

        public NotificationConstruction<T, TProperty> Changing(Action onPropertyChanging)
        {
            _notificationConstructor.Changing(onPropertyChanging);
            return Construction();
        }
        public NotificationConstruction<T, TProperty> Changing(OnPropertyChanging<TProperty> onPropertyChanging)
        {
            _notificationConstructor.Changing(onPropertyChanging);
            return Construction();
        }
        public NotificationConstruction<T, TProperty> Changed(Action onPropertyChanged)
        {
            _notificationConstructor.Changed(onPropertyChanged);
            return Construction();
        }
        public NotificationConstruction<T, TProperty> Changed(OnPropertyChanged<TProperty> onPropertyChanged)
        {
            _notificationConstructor.Changed(onPropertyChanged);
            return Construction();
        }

        private NotificationConstruction<T, TProperty> Construction()
        {
            return new NotificationConstruction<T, TProperty>(_notificator, this);
        }
    }
}