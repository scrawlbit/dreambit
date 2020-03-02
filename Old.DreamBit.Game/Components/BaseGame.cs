using System.IO;
using DreamBit.Game.Content;
using DreamBit.Game.Data;
using DreamBit.Game.Drawing;
using Scrawlbit.Json;
using Scrawlbit.Json.Converters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scrawlbit.Injection;
using Scrawlbit.Injection.Configuration;
using DreamBit.Game.Properties;

namespace DreamBit.Game.Components
{
    public class BaseGame : Microsoft.Xna.Framework.Game
    {
        private IContentManagerService _contentManager;
        private ISceneManager _sceneManager;
        private IDrawBatchService _drawBatch;
        private ICameraService _cameraService;
        private IGameScene _gameScene;

        protected BaseGame()
        {
            Graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }

        public static IContainer Container { get; private set; }
        private GameData GameData { get; set; }
        protected DebugData DebugData { get; private set; }
        protected GraphicsDeviceManager Graphics { get; }

        protected override void Initialize()
        {
            InitializeContainer();
            InitializeServices();
            LoadData();

            base.Initialize();
        }
        protected override void LoadContent()
        {
            _drawBatch.SpriteBatch = new SpriteBatch(GraphicsDevice);

            LoadStartScene(_sceneManager);
        }
        protected override void Update(GameTime gameTime)
        {
            _gameScene.Update(gameTime);

            PostUpdate(gameTime);
        }
        protected virtual void PostUpdate(GameTime gameTime)
        {
            _gameScene.PostUpdate(gameTime);
            _cameraService.UpdateSceneCamera();
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _gameScene.Draw(gameTime);

            base.Draw(gameTime);
        }

        private void InitializeContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegistrationBuilder.RegisterModule<GameInjectionModule>();
            RegisterModules(builder.RegistrationBuilder);

            Container = builder.Build();
        }
        private void InitializeServices()
        {
            Container.Inject(out _contentManager);
            Container.Inject(out _sceneManager);
            Container.Inject(out _drawBatch);
            Container.Inject(out _cameraService);
            Container.Inject(out _gameScene);

            _contentManager.ContentManager = Content;
        }
        private void LoadData()
        {
            var jsonParser = new JsonParser();

            jsonParser.Converters.Add(new DependencyInstanceConverter<DebugData>(Container));
            jsonParser.Converters.Add(new DependencyInstanceConverter<GameData>(Container));

            GameData = jsonParser.ParseString<GameData>(File.ReadAllText("Game.data"));
            DebugData = jsonParser.ParseString<DebugData>(File.ReadAllText("GameData.debug"));
        }

        protected virtual void RegisterModules(IRegistrationBuilder builder)
        {
        }
        protected virtual void LoadStartScene(ISceneManager sceneManager)
        {
            sceneManager.Load(GameData.StartScene);
        }
    }
}