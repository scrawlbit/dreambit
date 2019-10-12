using System;
using System.Linq;
using DreamBit.Game.Components;
using DreamBit.Game.Data;
using DreamBit.Game.Drawing;
using DreamBit.Game.Elements.Components;
using Scrawlbit.Injection;

namespace DreamBit.Game.Factory
{
    internal interface IGameObjectComponentFactory
    {
        GameObjectComponent CreateImageRenderer();
        GameObjectComponent CreateTextRenderer();
        GameObjectComponent CreateCameraObject();
        GameObjectComponent CreateScriptBehavior(string fileId);
    }

    internal class GameObjectComponentFactory : IGameObjectComponentFactory
    {
        private readonly IContainer _container;
        private readonly IGameData _gameData;

        public GameObjectComponentFactory(IContainer container, IGameData gameData)
        {
            _container = container;
            _gameData = gameData;
        }

        public GameObjectComponent CreateImageRenderer()
        {
            return new ImageRenderer(_container.Resolve<IDrawBatch>());
        }
        public GameObjectComponent CreateTextRenderer()
        {
            return new TextRenderer(_container.Resolve<IDrawBatch>());
        }
        public GameObjectComponent CreateCameraObject()
        {
            return new CameraObject(_container.Resolve<ICameraService>());
        }
        public GameObjectComponent CreateScriptBehavior(string fileId)
        {
            var scriptReference = _gameData.ScriptTypes.Single(q => q.FileId == Guid.Parse(fileId));

            return (ScriptBehavior)_container.Resolve(scriptReference.Type);
        }
    }
}