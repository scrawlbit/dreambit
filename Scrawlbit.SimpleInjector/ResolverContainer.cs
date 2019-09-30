using System;
using SimpleInjector;

namespace Scrawlbit.Injection
{
    internal class ResolverContainer : IContainer
    {
        private readonly Container _container;

        public ResolverContainer(Container container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }
        public object Resolve(Type serviceType)
        {
            return _container.GetInstance(serviceType);
        }
        public TService Resolve<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }
        public TService Inject<TService>(out TService variable) where TService : class
        {
            return variable = Resolve<TService>();
        }
    }
}