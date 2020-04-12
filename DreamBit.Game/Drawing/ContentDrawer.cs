using DreamBit.Game.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Drawing
{
    public interface IContentDrawer
    {
        SpriteBatch SpriteBatch { get; set; }
        GraphicsDevice GraphicsDevice { get; }

        void Begin();
        void End();

        void Draw(Image image, Rectangle rectangle, Color color);
        void Draw(Image image, Vector2 position, Color color);
        void Draw(Image image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect = SpriteEffects.None, float depth = 0);
        void Draw(Image image, Vector2 position, Rectangle? sourceRectangle = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, SpriteEffects effect = SpriteEffects.None, float depth = 0);
    }

    internal class ContentDrawer : IContentDrawer
    {
        public SpriteBatch SpriteBatch { get; set; }
        public GraphicsDevice GraphicsDevice => SpriteBatch.GraphicsDevice;

        public void Begin()
        {
            SpriteBatch.Begin();
        }
        public void End()
        {
            SpriteBatch.End();
        }

        public void Draw(Image image, Rectangle rectangle, Color color)
        {
            SpriteBatch.Draw(image.Texture, rectangle, color);
        }
        public void Draw(Image image, Vector2 position, Color color)
        {
            SpriteBatch.Draw(image.Texture, position, color);
        }
        public void Draw(Image image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth)
        {
            SpriteBatch.Draw(image.Texture, position, sourceRectangle, color, rotation, origin, scale, effect, depth);
        }
        public void Draw(Image image, Vector2 position, Rectangle? sourceRectangle, Color? color, float rotation, Vector2? origin, Vector2? scale, SpriteEffects effect, float depth)
        {
            SpriteBatch.Draw(
                image.Texture,
                position,
                sourceRectangle,
                color ?? Color.White,
                rotation,
                origin ?? Vector2.Zero,
                scale ?? Vector2.One,
                effect,
                depth
            );
        }
    }
}
