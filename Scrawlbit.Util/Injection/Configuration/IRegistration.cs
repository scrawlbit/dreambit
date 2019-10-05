using System;

namespace Scrawlbit.Injection.Configuration
{
    public interface IRegistration<in TService> where TService : class
    {
        bool Registered { get; }

        void Resolve<TImplementation>() where TImplementation : class, TService;

        void Transient<TImplementation>() where TImplementation : class, TService;
        void Transient<TImplementation>(Func<TImplementation> creator) where TImplementation : class, TService;
        void Transient<TImplementation>(Func<IContainer, TImplementation> creator) where TImplementation : class, TService;

        void Singleton();
        void Singleton<TImplementation>() where TImplementation : class, TService;
        void Singleton<TImplementation>(TImplementation instance) where TImplementation : class, TService;
        void Singleton<TImplementation>(Func<TImplementation> creator) where TImplementation : class, TService;
        void Singleton<TImplementation>(Func<IContainer, TImplementation> creator) where TImplementation : class, TService;
    }
}