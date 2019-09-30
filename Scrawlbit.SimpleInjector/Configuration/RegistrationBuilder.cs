using SimpleInjector;

namespace ScrawlBit.Injection.Configuration
{
    internal class RegistrationBuilder : IRegistrationBuilder
    {
        private readonly Container _container;
        private readonly IContainer _resolver;
        private readonly RegistrationSource _registrationSource;
        private IRegistration _lastRegistration;

        public RegistrationBuilder(Container container, IContainer resolver)
        {
            _container = container;
            _resolver = resolver;
            _registrationSource = new RegistrationSource(container);
        }

        public IRegistration<TService> Register<TService>() where TService : class
        {
            FinishLastRegistration();
            _lastRegistration = new Registration<TService>(_container, _resolver, _registrationSource);

            return (Registration<TService>)_lastRegistration;
        }
        public void RegisterModule(IInjectionModule module)
        {
            module.Register(this);
        }
        public void RegisterModule<T>() where T : IInjectionModule, new()
        {
            RegisterModule(new T());
        }

        public void Build()
        {
            FinishLastRegistration();
        }
        private void FinishLastRegistration()
        {
            if (_lastRegistration?.Registered == false)
                _lastRegistration.Self();
        }
    }
}