using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace ScrawlBit.Notification.Notificator
{
    internal class Notificator
    {
        private readonly List<PropertyChangingEventHandler> _propertyChangingHandlers;
        private readonly List<PropertyChangedEventHandler> _propertyChangedHandlers;
        private readonly List<ChainedNotificator> _chainedNotificators;
        private readonly List<ChainedNotificationConstructor> _chainedConstructors;
        private object _target;

        public Notificator(Type targetType)
        {
            TargetType = targetType;
            _propertyChangingHandlers = new List<PropertyChangingEventHandler>();
            _propertyChangedHandlers = new List<PropertyChangedEventHandler>();
            _chainedNotificators = new List<ChainedNotificator>();
            _chainedConstructors = new List<ChainedNotificationConstructor>();
        }

        private INotifyPropertyChanging ChangingImplementation => Target as INotifyPropertyChanging;
        private INotifyPropertyChanged ChangedImplementation => Target as INotifyPropertyChanged;
        public Type TargetType { get; }
        public virtual object Target
        {
            get { return _target; }
            set
            {
                if (value == _target) return;

                ChangingImplementation?.RemoveHandlers(_propertyChangingHandlers);
                ChangedImplementation?.RemoveHandlers(_propertyChangedHandlers);

                _target = value;

                ChangingImplementation?.AddHandlers(_propertyChangingHandlers);
                ChangedImplementation?.AddHandlers(_propertyChangedHandlers);

                _chainedConstructors.ForEach(c => c.UpdateChain());
                _chainedNotificators.ForEach(c => c.UpdateChain());
            }
        }

        internal void AddHandler(PropertyChangingEventHandler handler)
        {
            _propertyChangingHandlers.Add(handler);
            ChangingImplementation?.AddHandler(handler);
        }
        internal void AddHandler(PropertyChangedEventHandler handler)
        {
            _propertyChangedHandlers.Add(handler);
            ChangedImplementation?.AddHandler(handler);
        }

        private Notificator GetChain(PropertyInfo property)
        {
            var chain = _chainedNotificators.SingleOrDefault(c => c.ChainedProperty == property);

            if (chain == null)
                _chainedNotificators.Add(chain = new ChainedNotificator(this, property));

            return chain.Notificator;
        }
        private ChainedNotificationConstructor OnChainedProperty(Notificator initialNotificator, PropertyInfo property)
        {
            var constructor = new ChainedNotificationConstructor(initialNotificator, this, property);
            _chainedConstructors.Add(constructor);

            return constructor;
        }
        public virtual NotificationConstructor On(params PropertyInfo[] chainedProperty)
        {
            if (chainedProperty.Length == 1)
                return new NotificationConstructor(this, chainedProperty.Single());

            var chain = this;

            for (var i = 0; i < chainedProperty.Length - 1; i++)
                chain = chain.GetChain(chainedProperty[i]);

            return chain.OnChainedProperty(this, chainedProperty.Last());
        }

        public void Dispose()
        {
            ChangingImplementation?.RemoveHandlers(_propertyChangingHandlers);
            ChangedImplementation?.RemoveHandlers(_propertyChangedHandlers);

            _chainedNotificators.ForEach(c => c.Dispose());
        }
    }

    public interface INotificator : IDisposable
    {
    }

    public class Notificator<T> : INotificator
    {
        private readonly Notificator _notificator;

        internal Notificator(T target)
        {
            _notificator = new Notificator(typeof(T))
            {
                Target = target
            };
        }

        public NotificationConstructor<T, TProperty> On<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var m = (MemberExpression)expression.Body;
            var properties = new List<PropertyInfo>();

            while (m != null)
            {
                properties.Insert(0, (PropertyInfo)m.Member);
                m = m.Expression as MemberExpression;
            }

            return new NotificationConstructor<T, TProperty>(this, _notificator.On(properties.ToArray()));
        }

        public void Dispose()
        {
            _notificator.Dispose();
        }
    }
}