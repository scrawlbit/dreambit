using DreamBit.Game.Factory;
using DreamBit.Game.Writing;
using Scrawlbit.Injection.Configuration;

namespace DreamBit.Game.Properties
{
    internal class GameEditionInjectionModule : IInjectionModule
    {
        private IRegistrationBuilder _builder;

        public void Register(IRegistrationBuilder builder)
        {
            _builder = builder;

            Factory();
            Writing();
        }

        private void Factory()
        {
            _builder.Register<IGameObjectComponentFactory>().Transient<EditableGameObjectComponentFactory>();
        }
        private void Writing()
        {
            _builder.Register<IDataWriter>().Transient<DataWriter>();
        }
    }
}