using System;

namespace Scrawlbit.Injection
{
    public interface IContainer : IServiceProvider
    {
        object Resolve(Type serviceType);
        TService Resolve<TService>(Type serviceType) where TService : class;
        TService Resolve<TService>() where TService : class;
        TService Inject<TService>(out TService variable) where TService : class;
    }
}