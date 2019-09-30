using System;

namespace Scrawlbit.Injection
{
    public interface IContainer : IServiceProvider
    {
        object Resolve(Type serviceType);
        TService Resolve<TService>() where TService: class ;
        TService Inject<TService>(out TService variable) where TService : class;
    }
}