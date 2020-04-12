using DreamBit.Game.Content;
using DreamBit.Game.Registrations;
using DreamBit.Game.Serialization;
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

            Content();
            Registrations();
            Serialization();
        }

        private void Content()
        {
            _builder.Register<IContentFactory>().Singleton<ContentFactory>();
        }
        private void Registrations()
        {
            _builder.RegisterFile<ISceneFileRegistration>().Transient<SceneFileRegistration>();
            _builder.RegisterFile<IScriptFileRegistration>().Transient<ScriptFileRegistration>();
        }
        private void Serialization()
        {
            _builder.Register<IGameElementsParser>().Transient<GameElementsParser>();
        }
    }
}
