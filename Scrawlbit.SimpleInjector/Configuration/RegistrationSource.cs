using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrawlbit.Injection.Configuration
{
    internal class RegistrationSource
    {
        private readonly Container _container;
        private readonly List<Registration> _registrations;

        public RegistrationSource(Container container)
        {
            _container = container;
            _registrations = new List<Registration>();
        }

        public Registration Get<TImplementation>(Lifestyle lifeStyle, Func<TImplementation> creator = null) where TImplementation : class
        {
            var registration = _registrations.SingleOrDefault(r => r.ImplementationType == typeof(TImplementation));
            if (registration == null)
            {
                registration = creator != null
                    ? lifeStyle.CreateRegistration(creator, _container)
                    : lifeStyle.CreateRegistration<TImplementation>(_container);

                _registrations.Add(registration);
            }

            return registration;
        }
    }
}