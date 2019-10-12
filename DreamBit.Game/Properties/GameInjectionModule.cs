using DreamBit.Game.Components;
using DreamBit.Game.Content;
using DreamBit.Game.Data;
using DreamBit.Game.Drawing;
using DreamBit.Game.Elements.Components;
using DreamBit.Game.Factory;
using DreamBit.Game.Reading;
using DreamBit.Game.Reading.Converters;
using Scrawlbit.Json;
using Scrawlbit.Injection.Configuration;

namespace DreamBit.Game.Properties
{
    internal class GameInjectionModule : IInjectionModule
    {
        private IRegistrationBuilder _builder;

        public void Register(IRegistrationBuilder builder)
        {
            _builder = builder;

            Components();
            Content();
            Data();
            Drawing();
            Elements();
            Factory();
            Reading();
        }

        private void Components()
        {
            _builder.Register<ICameraService>().Singleton<CameraService>();
            _builder.Register<IGameScene>().Singleton<GameScene>();
            _builder.Register<ISceneCamera>().Singleton<SceneCamera>();
            _builder.Register<ISceneManager>().Singleton<SceneManager>();
        }
        private void Content()
        {
            _builder.Register<IContentManager>().Singleton<ContentManagerService>();
            _builder.Register<IContentManagerService>().Singleton<ContentManagerService>();
            _builder.Register<IContentReferenceManager>().Singleton<ContentReferenceManager>();
        }
        private void Data()
        {
            _builder.Register<GameData>().Singleton();
            _builder.Register<DebugData>().Singleton();

            _builder.Register<IGameData>().Singleton<GameData>();
            _builder.Register<IDebugData>().Singleton<DebugData>();
        }
        private void Drawing()
        {
            _builder.Register<IDrawBatch>().Singleton<DrawBatch>();
            _builder.Register<IDrawBatchService>().Singleton<DrawBatchService>();
        }
        private void Elements()
        {
            _builder.Register<CameraObject>().Transient(c => new CameraObject(c.Resolve<ICameraService>()));
            _builder.Register<ImageRenderer>().Transient(c => new ImageRenderer(c.Resolve<IDrawBatch>()));
            _builder.Register<TextRenderer>().Transient(c => new TextRenderer(c.Resolve<IDrawBatch>()));
        }
        private void Factory()
        {
            _builder.Register<IGameObjectComponentFactory>().Transient<GameObjectComponentFactory>();
        }
        private void Reading()
        {
            _builder.Register<IJsonParser>().Transient<JsonParser>();
            _builder.Register<IDataReader>().Singleton<DataReader>();
            
            _builder.Register<GameObjectComponentConverter>();
            _builder.Register<GameObjectConverter>();
        }
    }
}
