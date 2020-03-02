using DreamBit.Game.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Drawing
{
    public interface IDrawBatch
    {
        GraphicsDevice GraphicsDevice { get; }

        void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null);
        void End();

        void Draw(IImage image, Rectangle rectangle, Color color);
        void Draw(IImage image, Vector2 position, Color color);
        void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect = SpriteEffects.None, float depth = 0);
        void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, SpriteEffects effect = SpriteEffects.None, float depth = 0);
        
        void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float depth = 0);
        void DrawString(IFont font, string text, Vector2 position, Color color, float rotation = 0, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float depth = 0);
    }
}