using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Tests.Mocks.Drawing
{
    internal class DrawBatchServiceMock : IDrawBatchService
    {
        public SpriteBatch SpriteBatch { get; set; }
        public int BeginCount { get; set; }
        public int EndCount { get; set; }
        public DrawBatchDefinition LastBeginDefinition { get; set; }

        public void Begin(DrawBatchDefinition definition)
        {
            BeginCount++;
            LastBeginDefinition = definition;
        }
        public void End()
        {
            EndCount++;
        }

        public void Draw(IImage image, Rectangle rectangle, Color color)
        {
        }
        public void Draw(IImage image, Vector2 position, Color color)
        {
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth)
        {
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
        }

        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth)
        {
        }
        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
        }
    }
}