using DreamBit.Modularization.Management;
using DreamBit.Project.Serialization;
using Scrawlbit.Injection.Configuration;
using Scrawlbit.Json;

namespace DreamBit.Project.Properties
{
    public class ProjectInjectionModule : IInjectionModule
    {
        public void Register(IRegistrationBuilder builder)
        {
            builder.Register<IProject>().Singleton<Project>();

            builder.Register<ISerializer>().Transient(c =>
            {
                var jsonParser = new JsonParser();
                var fileManager = c.Resolve<IFileManager>();

                return new Serializer(jsonParser, fileManager);
            });
        }
    }
}