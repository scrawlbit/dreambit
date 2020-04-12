using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScrawlBit.MonoGame.Interop.Controls;

namespace DreamBit.Extension.Module
{
    public class TestGame : GameModule
    {
        private Texture2D _pixel;

        public Vector2 Position { get; set; }

        public override void Initialize()
        {
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }
        public override void Draw()
        {
            GraphicsDevice.Clear(Color.Transparent);

            SpriteBatch.Begin();
            SpriteBatch.Draw(_pixel, Position, new Rectangle((int)Position.X, (int)Position.Y, 40, 40), Color.Red * 0.5f);
            SpriteBatch.End();
        }
    }
}