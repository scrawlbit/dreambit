using DreamBit.Game.Registrations;
using DreamBit.Project.Helpers;
using Scrawlbit.Injection.Configuration;

namespace DreamBit.Game.Properties
{
    public class GameInjectionModule : IInjectionModule
    {
        private IRegistrationBuilder _builder;

        public void Register(IRegistrationBuilder builder)
        {
            _builder = builder;

            Registrations();
        }

        private void Registrations()
        {
            _builder.RegisterFile<ISceneFileRegistration>().Transient<SceneFileRegistration>();
            _builder.RegisterFile<IScriptFileRegistration>().Transient<ScriptFileRegistration>();
        }
    }
}
