namespace ScrawlBit.Injection.Configuration
{
    public interface IRegistrationBuilder
    {
        IRegistration<TService> Register<TService>() where TService : class;

        void RegisterModule(IInjectionModule module);
        void RegisterModule<T>() where T : IInjectionModule, new();
    }
}