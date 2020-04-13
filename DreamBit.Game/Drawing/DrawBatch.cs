using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DreamBit.Game.Drawing
{
    public interface IDrawBatch : IDisposable
    {
    }

    internal class DrawBatch : IDrawBatch
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteSortMode _sortMode;
        private readonly BlendState _blendState;
        private readonly SamplerState _samplerState;
        private readonly DepthStencilState _depthStencilState;
        private readonly RasterizerState _rasterizerState;
        private readonly Effect _effect;
        private readonly Matrix? _transformMatrix;

        public DrawBatch(
            SpriteBatch spriteBatch,
            SpriteSortMode? sortMode,
            BlendState blendState,
            SamplerState samplerState,
            DepthStencilState depthStencilState,
            RasterizerState rasterizerState,
            Effect effect,
            Matrix? transformMatrix)
        {
            _spriteBatch = spriteBatch;
            _sortMode = sortMode ?? SpriteSortMode.Deferred;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;
            _transformMatrix = transformMatrix;
        }

        public Action Ended;

        public void Dispose()
        {
            Ended?.Invoke();
        }

        public void Begin()
        {
            _spriteBatch.Begin(
                _sortMode,
                _blendState,
                _samplerState,
                _depthStencilState,
                _rasterizerState,
                _effect,
                _transformMatrix
            );
        }
        public void End()
        {
            _spriteBatch.End();
        }

        public bool Equals(DrawBatch other)
        {
            return
                other != null &&
                _sortMode == other._sortMode &&
                _blendState == other._blendState &&
                _samplerState == other._samplerState &&
                _depthStencilState == other._depthStencilState &&
                _rasterizerState == other._rasterizerState &&
                _effect == other._effect &&
                _transformMatrix == other._transformMatrix;
        }
    }
}
