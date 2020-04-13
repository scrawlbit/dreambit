using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Game.Drawing
{
    internal class DrawBatchData
    {
        public DrawBatchData(
            SpriteSortMode sortMode = SpriteSortMode.Deferred,
            BlendState blendState = null,
            SamplerState samplerState = null,
            DepthStencilState depthStencilState = null,
            RasterizerState rasterizerState = null,
            Effect effect = null,
            Matrix? transformMatrix = default
        )
        {
            SortMode = sortMode;
            BlendState = blendState ?? BlendState.AlphaBlend;
            SamplerState = samplerState ?? SamplerState.LinearClamp;
            DepthStencilState = depthStencilState ?? DepthStencilState.None;
            RasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            Effect = effect;
            TransformMatrix = transformMatrix;
        }

        public SpriteSortMode SortMode { get; }
        public BlendState BlendState { get; }
        public SamplerState SamplerState { get; }
        public DepthStencilState DepthStencilState { get; }
        public RasterizerState RasterizerState { get; }
        public Effect Effect { get; }
        public Matrix? TransformMatrix { get; }

        public override int GetHashCode()
        {
            var hash = 17;

            hash = hash * 23 + SortMode.GetHashCode();
            hash = hash * 23 + BlendState.GetHashCode();
            hash = hash * 23 + SamplerState.GetHashCode();
            hash = hash * 23 + DepthStencilState.GetHashCode();
            hash = hash * 23 + RasterizerState.GetHashCode();

            if (Effect != null)
                hash = hash * 23 + Effect.GetHashCode();

            if (TransformMatrix != null)
                hash = hash * 23 + TransformMatrix.GetHashCode();

            return hash;
        }
        public override bool Equals(object obj)
        {
            if (obj is DrawBatchData definition)
            {
                return SortMode == definition.SortMode &&
                       BlendState == definition.BlendState &&
                       SamplerState == definition.SamplerState &&
                       DepthStencilState == definition.DepthStencilState &&
                       RasterizerState == definition.RasterizerState &&
                       Effect == definition.Effect &&
                       TransformMatrix == definition.TransformMatrix;
            }

            return false;
        }

        public static bool operator ==(DrawBatchData a, DrawBatchData b)
        {
            return Equals(a, b);
        }
        public static bool operator !=(DrawBatchData a, DrawBatchData b)
        {
            return !(a == b);
        }
    }
}