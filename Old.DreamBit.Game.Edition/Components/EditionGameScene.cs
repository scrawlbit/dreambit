﻿using System.IO;
using DreamBit.Game.Content;
using DreamBit.Game.Data;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Scrawlbit.Injection;
using Scrawlbit.Json;
using Scrawlbit.Json.Converters;

namespace DreamBit.Game.Components
{
    public class EditionGameScene : IEditionGameScene
    {
        private readonly IContainer _container;
        private readonly IGameScene _gameScene;
        private readonly IContentManagerService _contentManagerService;
        private readonly IDrawBatchService _drawBatchService;
        private readonly ISceneManager _sceneManager;

        internal EditionGameScene(
            IContainer container,
            IGameScene gameScene,
            IContentManagerService contentManagerService,
            IDrawBatchService drawBatchService,
            ISceneManager sceneManager)
        {
            _container = container;
            _gameScene = gameScene;
            _contentManagerService = contentManagerService;
            _drawBatchService = drawBatchService;
            _sceneManager = sceneManager;
        }

        public ContentManager ContentManager
        {
            get => _contentManagerService.ContentManager;
            set => _contentManagerService.ContentManager = value;
        }
        public SpriteBatch SpriteBatch
        {
            get => _drawBatchService.SpriteBatch;
            set => _drawBatchService.SpriteBatch = value;
        }

        public void LoadContent()
        {
            var jsonParser = new JsonParser();

            jsonParser.Converters.Add(new DependencyInstanceConverter<GameData>(_container));
            jsonParser.ParseString<GameData>(File.ReadAllText("Game.data"));
        }
        public void Update(GameTime gameTime)
        {
            _gameScene.Update(gameTime);
        }
        public void PostUpdate(GameTime gameTime)
        {
            _gameScene.PostUpdate(gameTime);
        }
        public void Draw(GameTime gameTime)
        {
            _gameScene.Draw(gameTime);
        }

        public void OpenScene(string assetName)
        {
            _sceneManager.Load(assetName);
        }
    }
}