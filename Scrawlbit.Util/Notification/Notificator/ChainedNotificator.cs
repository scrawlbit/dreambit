using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace ScrawlBit.Notification.Notificator
{
    internal class ChainedNotificator : IDisposable
    {
        private readonly Notificator _baseNotificator;

        public ChainedNotificator(Notificator baseNotificator, PropertyInfo chainedProperty)
        {
            _baseNotificator = baseNotificator;

            ChainedProperty = chainedProperty;
            Notificator = new Notificator(chainedProperty.PropertyType);

            _baseNotificator.On(chainedProperty).Changed(UpdateChain);

            UpdateChain();
        }
        
        public PropertyInfo ChainedProperty { get; }
        public Notificator Notificator { get; }
        
        public void UpdateChain()
        {
            if (_baseNotificator.Target == null)
            {
                Notificator.Target = null;
                return;
            }
            
            Notificator.Target = ChainedProperty.GetValue(_baseNotificator.Target);
        }

        public void Dispose()
        {
            Notificator.Dispose();
        }
    }
}