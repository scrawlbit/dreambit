using System.Collections.Generic;
using System.Linq;
using DreamBit.Game.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Drawing
{
    internal class DrawBatch : IDrawBatch
    {
        private readonly IDrawBatchService _drawBatchService;
        private readonly Stack<DrawBatchDefinitionRequest> _definitionRequests;

        public DrawBatch(IDrawBatchService drawBatchService)
        {
            _drawBatchService = drawBatchService;
            _definitionRequests = new Stack<DrawBatchDefinitionRequest>();
        }

        public GraphicsDevice GraphicsDevice => _drawBatchService.SpriteBatch.GraphicsDevice;

        public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix? transformMatrix)
        {
            var definition = new DrawBatchDefinition(
                sortMode,
                blendState,
                samplerState,
                depthStencilState,
                rasterizerState,
                effect,
                transformMatrix
            );

            if (_definitionRequests.Any())
            {
                var mostRecentDefinitionRequest = _definitionRequests.First();

                if (definition == mostRecentDefinitionRequest.Definition)
                {
                    mostRecentDefinitionRequest.RequestCount++;
                    return;
                }

                _drawBatchService.End();
            }

            _definitionRequests.Push(new DrawBatchDefinitionRequest(definition));
            _drawBatchService.Begin(definition);
        }
        public void End()
        {
            if (!_definitionRequests.Any())
                return;

            var mostRecentDefinitionRequest = _definitionRequests.First();

            mostRecentDefinitionRequest.RequestCount--;
            if (mostRecentDefinitionRequest.RequestCount > 0)
                return;

            _drawBatchService.End();
            _definitionRequests.Pop();

            if (!_definitionRequests.Any())
                return;

            _drawBatchService.Begin(_definitionRequests.First().Definition);
        }

        public void Draw(IImage image, Rectangle rectangle, Color color)
        {
            _drawBatchService.Draw(image, rectangle, color);
        }
        public void Draw(IImage image, Vector2 position, Color color)
        {
            _drawBatchService.Draw(image, position, color);
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth)
        {
            _drawBatchService.Draw(image, position, sourceRectangle, color, rotation, origin, scale, effect, depth);
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color? color, float rotation, Vector2? origin, Vector2? scale, SpriteEffects effect, float depth)
        {
            _drawBatchService.Draw(
                image,
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

        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
        {
            _drawBatchService.DrawString(font, text, position, color, rotation, origin, scale, effects, depth);
        }
        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2? origin, Vector2? scale, SpriteEffects effects, float depth)
        {
            _drawBatchService.DrawString(
                font,
                text,
                position,
                color,
                rotation,
                origin ?? Vector2.Zero,
                scale ?? Vector2.One,
                effects,
                depth
            );
        }
    }
}