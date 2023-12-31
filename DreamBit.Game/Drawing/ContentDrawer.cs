﻿using DreamBit.Game.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scrawlbit.MonoGame.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Game.Drawing
{
    public interface IContentDrawer
    {
        SpriteBatch SpriteBatch { get; set; }
        GraphicsDevice GraphicsDevice { get; }
        Texture2D Pixel { get; }

        IDrawBatch Batch(SpriteSortMode? sortMode = null, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null);

        void Draw(IImage image, Vector2 position, Color color);
        void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect = SpriteEffects.None, float depth = 0);
        void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, SpriteEffects effect = SpriteEffects.None, float depth = 0);

        void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects = SpriteEffects.None, float depth = 0);
        void DrawString(IFont font, string text, Vector2 position, Color color, float rotation = 0, Vector2? origin = null, Vector2? scale = null, SpriteEffects effects = SpriteEffects.None, float depth = 0);

        void DrawLine(Vector2 position, float length, float angle, Color color, float thickness = 1f);
        void DrawLine(Vector2 a, Vector2 b, Color color, float thickness = 1f);

        void FillRectangle(Rectangle rectangle, Color color);
        void FillRectangle(Vector2 position, Vector2 size, Color color);
        void DrawRectangle(Rectangle rect, Color color, float thickness = 1f);
        void DrawRectangle(Vector2 position, Vector2 size, Color color, float thickness = 1f);

        void FillTriangle(Vector2 a, Vector2 b, Vector2 c, Color color);

        void DrawCircle(Vector2 center, float radius, int sides, Color color, float thickness = 1f);
    }

    internal class ContentDrawer : IContentDrawer
    {
        private readonly ICollection<DrawBatch> _batches;

        public ContentDrawer()
        {
            _batches = new List<DrawBatch>();
        }

        public SpriteBatch SpriteBatch { get; set; }
        public GraphicsDevice GraphicsDevice => SpriteBatch.GraphicsDevice;
        public Texture2D Pixel => SpriteBatch.GetPixel();

        public IDrawBatch Batch(SpriteSortMode? sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix? transformMatrix)
        {
            var previous = _batches.LastOrDefault();
            var next = new DrawBatch(SpriteBatch, sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);

            if (!next.Equals(previous))
            {
                previous?.End();
                next.Begin();
                _batches.Add(next);

                next.Ended = () =>
                {
                    next.End();
                    previous?.Begin();

                    _batches.Remove(next);
                };
            }

            return next;
        }

        public void Draw(IImage image, Vector2 position, Color color)
        {
            SpriteBatch.Draw(image.Texture, position, color);
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, float depth)
        {
            SpriteBatch.Draw(image.Texture, position, sourceRectangle, color, rotation, origin, scale, effect, depth);
        }
        public void Draw(IImage image, Vector2 position, Rectangle? sourceRectangle, Color? color, float rotation, Vector2? origin, Vector2? scale, SpriteEffects effect, float depth)
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

        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
        {
            SpriteBatch.DrawString(font.SpriteFont, text, position, color, rotation, origin, scale, effects, depth);
        }
        public void DrawString(IFont font, string text, Vector2 position, Color color, float rotation, Vector2? origin, Vector2? scale, SpriteEffects effects, float depth)
        {
            SpriteBatch.DrawString(
                font.SpriteFont,
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

        public void DrawLine(Vector2 position, float length, float angle, Color color, float thickness)
        {
            SpriteBatch.DrawLine(position, length, angle, color, thickness);
        }
        public void DrawLine(Vector2 a, Vector2 b, Color color, float thickness)
        {
            SpriteBatch.DrawLine(a, b, color, thickness);
        }

        public void FillRectangle(Rectangle rectangle, Color color)
        {
            SpriteBatch.FillRectangle(rectangle, color);
        }
        public void FillRectangle(Vector2 position, Vector2 size, Color color)
        {
            SpriteBatch.FillRectangle(position, size, color);
        }
        public void DrawRectangle(Rectangle rect, Color color, float thickness)
        {
            SpriteBatch.DrawRectangle(rect, color, thickness);
        }
        public void DrawRectangle(Vector2 position, Vector2 size, Color color, float thickness)
        {
            SpriteBatch.DrawRectangle(position, size, color, thickness);
        }

        public void FillTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            SpriteBatch.FillTriangle(a, b, c, color);
        }

        public void DrawCircle(Vector2 center, float radius, int sides, Color color, float thickness = 1)
        {
            SpriteBatch.DrawCircle(center, radius, sides, color, thickness);
        }
    }
}
