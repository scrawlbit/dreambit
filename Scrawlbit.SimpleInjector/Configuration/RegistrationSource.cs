using System;
using System.Collections.Generic;
using System.Linq;
using SimpleInjector;
using SimpleInjector.Diagnostics;

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

                registration.SuppressDiagnosticWarning(DiagnosticType.LifestyleMismatch, "Lifestyle hierachy validation is ignored for Scrawlbit containers");

                _registrations.Add(registration);
            }

            return registration;
        }
    }
}