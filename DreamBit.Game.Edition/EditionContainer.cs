using DreamBit.Game.Properties;
using Scrawlbit.Injection;

namespace DreamBit.Game
{
    internal static class EditionContainer
    {
        public static readonly IContainer Container;

        static EditionContainer()
        {
            var builder = new ContainerBuilder { AllowOverrides = true };

            builder.RegistrationBuilder.RegisterModule<GameInjectionModule>();
            builder.RegistrationBuilder.RegisterModule<GameEditionInjectionModule>();

            Container = builder.Build();
        }

        public static T Resolve<T>() where T : class
        {
            return Container.Resolve<T>();
        }
    }
}