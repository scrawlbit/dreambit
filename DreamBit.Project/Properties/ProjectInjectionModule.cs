using DreamBit.Project.Registrations;
using DreamBit.Project.Serialization;
using Scrawlbit.Injection.Configuration;

namespace DreamBit.Project.Properties
{
    public class ProjectInjectionModule : IInjectionModule
    {
        public void Register(IRegistrationBuilder builder)
        {
            builder.Register<Project>().Singleton();
            builder.Register<IProject>().Resolve<Project>();
            builder.Register<IProjectManager>().Resolve<Project>();

            builder.Register<ISerializer>().Transient<Serializer>();

            builder.Register<IFileRegistrations>().Singleton<FileRegistrations>();
        }
    }
}