using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Tests.Mocks.Drawing
{
    public class DrawBatchMock : IDrawBatch
    {
        public int DrawCount { get; set; }
        public int DrawStringCount { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }

        public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
        {
        }
        public void End()
        {
        }

        public void Draw(IImage image, Rectangle rectangle, Color color)
        {
            DrawCount++;
        }
        public void Draw(IImage image, Vector2 position, Color color)
        {
            DrawCount++;
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect = SpriteEffects.None, float depth = 0)
        {
            DrawCount++;
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color? color, float rotation, Vector2? origin, Vector2? scale, SpriteEffects effect, float depth)
        {
            DrawCount++;
        }

        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
        {
            DrawStringCount++;
        }
        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2? origin, Vector2? scale, SpriteEffects effects, float depth)
        {
            DrawStringCount++;
        }
    }
}