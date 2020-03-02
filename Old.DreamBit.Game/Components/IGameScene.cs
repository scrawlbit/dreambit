using Microsoft.Xna.Framework;

namespace DreamBit.Game.Components
{
    internal interface IGameScene
    {
        void Update(GameTime gameTime);
        void PostUpdate(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}