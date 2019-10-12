using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Components
{
    public interface IEditionGameScene
    {
        ContentManager ContentManager { get; set; }
        SpriteBatch SpriteBatch { get; set; }

        void LoadContent();
        void Update(GameTime gameTime);
        void PostUpdate(GameTime gameTime);
        void Draw(GameTime gameTime);

        void OpenScene(string assetName);
    }
}