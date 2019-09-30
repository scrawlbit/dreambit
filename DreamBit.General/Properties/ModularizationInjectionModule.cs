using DreamBit.Modularization.Management;
using ScrawlBit.Injection.Configuration;

namespace DreamBit.Modularization.Properties
{
    public class ModularizationInjectionModule : IInjectionModule
    {
        private IRegistrationBuilder _builder;

        public void Register(IRegistrationBuilder builder)
        {
            _builder = builder;

            Components();
        }

        private void Components()
        {
            _builder.Register<IFileManager>().Transient<FileManager>();
        }
    }
}