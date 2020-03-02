using DreamBit.Game.Factory;
using DreamBit.Game.Registrations;
using DreamBit.Game.Writing;
using DreamBit.Project.Helpers;
using Scrawlbit.Injection.Configuration;

namespace DreamBit.Game.Properties
{
    public class GameEditionInjectionModule : IInjectionModule
    {
        private IRegistrationBuilder _builder;

        public void Register(IRegistrationBuilder builder)
        {
            _builder = builder;

            new GameInjectionModule().Register(builder);

            Factory();
            Registrations();
            Writing();
        }

        private void Factory()
        {
            _builder.Register<IGameObjectComponentFactory>().Transient<EditableGameObjectComponentFactory>();
        }
        private void Registrations()
        {
            _builder.RegisterFile<ISceneFileRegistration>().Transient<SceneFileRegistration>();
            _builder.RegisterFile<IScriptFileRegistration>().Transient<ScriptFileRegistration>();
        }
        private void Writing()
        {
            _builder.Register<ISceneWriter>().Transient<SceneWriter>();
            _builder.Register<IDataWriter>().Transient<DataWriter>();
        }
    }
}