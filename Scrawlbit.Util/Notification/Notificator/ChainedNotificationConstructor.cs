using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Scrawlbit.Notification.Notificator
{
    internal class ChainedNotificationConstructor : NotificationConstructor
    {
        private readonly Notificator _initialNotificator;
        private readonly List<OnPropertyChanged<object>> _propertyChangingHandlers;
        private readonly List<OnPropertyChanged<object>> _propertyChangedHandlers;
        private object _value;

        public ChainedNotificationConstructor(Notificator initialNotificator, Notificator notificator, PropertyInfo property) : base(notificator, property)
        {
            _initialNotificator = initialNotificator;
            _propertyChangingHandlers = new List<OnPropertyChanged<object>>();
            _propertyChangedHandlers = new List<OnPropertyChanged<object>>();

            notificator.On(property).Changing<object>((o, n) =>
            {
                _value = n;
            });

            UpdateChain();
        }

        private object Value
        {
            set
            {
                if (Equals(value, _value)) return;

                _propertyChangingHandlers.ForEach(p => p(_value, value));
                _propertyChangedHandlers.ForEach(p => p(_value, value));

                _value = value;
            }
        }

        public override NotificationConstruction Changing(Action onPropertyChanging)
        {
            _propertyChangingHandlers.Add((o, n) => onPropertyChanging());
            return base.Changing(onPropertyChanging);
        }
        public override NotificationConstruction Changing<TProperty>(OnPropertyChanging<TProperty> onPropertyChanging)
        {
            _propertyChangingHandlers.Add((o, n) => onPropertyChanging((TProperty)o, (TProperty)n));
            return base.Changing(onPropertyChanging);
        }
        public override NotificationConstruction Changed(Action onPropertyChanged)
        {
            _propertyChangedHandlers.Add((o, n) => onPropertyChanged());
            return base.Changed(onPropertyChanged);
        }
        public override NotificationConstruction Changed<TProperty>(OnPropertyChanged<TProperty> onPropertyChanged)
        {
            _propertyChangedHandlers.Add((o, n) => onPropertyChanged((TProperty)o, (TProperty)n));
            return base.Changed(onPropertyChanged);
        }

        protected override NotificationConstruction Construction()
        {
            return new NotificationConstruction(_initialNotificator, this);
        }
        
        public void UpdateChain()
        {
            if (Notificator.Target != null)
            {
                Value = Property.GetValue(Notificator.Target);
            }
            else if (Property.PropertyType.IsValueType)
            {
                Value = Activator.CreateInstance(Property.PropertyType);
            }
            else
            {
                Value = null;
            }
        }
    }
}