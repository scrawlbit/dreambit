using System;
using SimpleInjector;

namespace ScrawlBit.Injection.Configuration
{
    internal interface IRegistration
    {
        bool Registered { get; }

        void Self();
    }

    internal class Registration<TService> : IRegistration, IRegistration<TService> where TService : class
    {
        private readonly Container _container;
        private readonly IContainer _resolver;
        private readonly RegistrationSource _registrations;

        public Registration(Container container, IContainer resolver, RegistrationSource registrations)
        {
            _container = container;
            _resolver = resolver;
            _registrations = registrations;
        }

        public bool Registered { get; private set; }

        public void Self()
        {
            Register(_registrations.Get<TService>(Lifestyle.Transient));
        }

        public void Transient<TImplementation>() where TImplementation : class, TService
        {
            Register(_registrations.Get<TImplementation>(Lifestyle.Transient));
        }
        public void Transient<TImplementation>(Func<TImplementation> creator) where TImplementation : class, TService
        {
            Register(_registrations.Get(Lifestyle.Transient, creator));
        }
        public void Transient<TImplementation>(Func<IContainer, TImplementation> creator) where TImplementation : class, TService
        {
            var container = _resolver;
            
            Register(_registrations.Get(Lifestyle.Transient, () => creator(container)));
        }

        public void Singleton()
        {
            Register(_registrations.Get<TService>(Lifestyle.Singleton));
        }
        public void Singleton<TImplementation>() where TImplementation : class, TService
        {
            Register(_registrations.Get<TImplementation>(Lifestyle.Singleton));
        }
        public void Singleton<TImplementation>(TImplementation instance) where TImplementation : class, TService
        {
            Register(_registrations.Get(Lifestyle.Singleton, () => instance));
        }
        public void Singleton<TImplementation>(Func<TImplementation> creator) where TImplementation : class, TService
        {
            Register(_registrations.Get(Lifestyle.Singleton, creator));
        }
        public void Singleton<TImplementation>(Func<IContainer, TImplementation> creator) where TImplementation : class, TService
        {
            var container = _resolver;

            Register(_registrations.Get(Lifestyle.Singleton, () => creator(container)));
        }

        private void Register(Registration registration)
        {
            if (Registered)
                throw new Exception("This registration was already done.");

            _container.AddRegistration(typeof(TService), registration);
            Registered = true;
        }
    }
}