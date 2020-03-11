using DreamBit.General.State;
using DreamBit.Modularization.Management;
using Scrawlbit.Injection.Configuration;
using Scrawlbit.Json;

namespace DreamBit.Modularization.Properties
{
    public class GeneralInjectionModule : IInjectionModule
    {
        private IRegistrationBuilder _builder;

        public void Register(IRegistrationBuilder builder)
        {
            _builder = builder;

            Management();
            State();
            Other();
        }

        private void Management()
        {
            _builder.Register<IFileManager>().Singleton<FileManager>();
        }
        private void State()
        {
            _builder.Register<IStateManager>().Singleton<StateManager>();
        }
        private void Other()
        {
            _builder.Register<IJsonParser>().Transient<JsonParser>();
        }
    }
}