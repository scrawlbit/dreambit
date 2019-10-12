using DreamBit.Game.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Drawing
{
    internal class DrawBatchService : IDrawBatchService
    {
        public SpriteBatch SpriteBatch { get; set; }

        public void Begin(DrawBatchDefinition definition)
        {
            SpriteBatch.Begin(
                definition.SortMode,
                definition.BlendState,
                definition.SamplerState,
                definition.DepthStencilState,
                definition.RasterizerState,
                definition.Effect,
                definition.TransformMatrix
            );
        }
        public void End()
        {
            SpriteBatch.End();
        }

        public void Draw(IImage image, Rectangle rectangle, Color color)
        {
            SpriteBatch.Draw(image.Texture, rectangle, color);
        }
        public void Draw(IImage image, Vector2 position, Color color)
        {
            SpriteBatch.Draw(image.Texture, position, color);
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
        {
            SpriteBatch.Draw(image.Texture, position, sourceRectangle, color, rotation, origin, scale, effects, depth);
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float depth)
        {
            SpriteBatch.Draw(image.Texture, position, sourceRectangle, color, rotation, origin, scale, effects, depth);
        }

        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
        {
            SpriteBatch.DrawString(font.SpriteFont, text, position, color, rotation, origin, scale, effects, depth);
        }
        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float depth)
        {
            SpriteBatch.DrawString(font.SpriteFont, text, position, color, rotation, origin, scale, effects, depth);
        }
    }
}