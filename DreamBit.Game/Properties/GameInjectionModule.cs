using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
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
            Drawing();
            Registrations();
            Serialization();
        }

        private void Content()
        {
            _builder.Register<IContentManager>().Singleton<ContentManager>();
            _builder.Register<IContentLoader>().Singleton<ContentManager>();
        }
        private void Drawing()
        {
            _builder.Register<IContentDrawer>().Singleton<ContentDrawer>();
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
