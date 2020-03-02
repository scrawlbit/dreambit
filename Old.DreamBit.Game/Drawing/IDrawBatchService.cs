using DreamBit.Game.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Drawing
{
    internal interface IDrawBatchService
    {
        SpriteBatch SpriteBatch { get; set; }

        void Begin(DrawBatchDefinition definition);
        void End();

        void Draw(IImage image, Rectangle rectangle, Color color);
        void Draw(IImage image, Vector2 position, Color color);
        void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth);
        void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth);

        void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth);
        void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth);
    }
}