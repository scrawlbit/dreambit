using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Components
{
    internal class GameScene : IGameScene
    {
        private readonly ISceneManager _sceneManager;
        private readonly IDrawBatch _drawBatch;
        private readonly ISceneCamera _sceneCamera;

        public GameScene(ISceneManager sceneManager, IDrawBatch drawBatch, ISceneCamera sceneCamera)
        {
            _sceneManager = sceneManager;
            _drawBatch = drawBatch;
            _sceneCamera = sceneCamera;
        }
        
        public void Update(GameTime gameTime)
        {
            DeltaTime.GameTime = gameTime;

            _sceneManager.OpenedScene.Update();
        }
        public void PostUpdate(GameTime gameTime)
        {
            _sceneManager.OpenedScene.PostUpdate();
        }
        public void Draw(GameTime gameTime)
        {
            _drawBatch.Begin(transformMatrix: _sceneCamera.TransformMatrix);
            _sceneManager.OpenedScene.Draw();
            _drawBatch.End();
        }
    }
}