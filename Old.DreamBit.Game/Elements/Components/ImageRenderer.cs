using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using DreamBit.Game.Reading.Attributes;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Elements.Components
{
    public interface IImageRenderer
    {
        Color Color { get; set; }
        bool FlipVertically { get; set; }
        bool FlipHorizontally { get; set; }
        Vector2 Origin { get; set; }
        IImage Image { get; set; }
    }

    internal class ImageRenderer : GameObjectComponent, IImageRenderer
    {
        private readonly IDrawBatch _drawBatch;

        internal ImageRenderer(IDrawBatch drawBatch)
        {
            _drawBatch = drawBatch;
        }

        public Color Color { get; set; }
        public bool FlipVertically { get; set; }
        public bool FlipHorizontally { get; set; }
        public Vector2 Origin { get; set; }
        [ContentReference("FileId")]
        public IImage Image { get; set; }
        
        protected internal override void Draw()
        {
            if (Image == null)
                return;

            _drawBatch.Draw(
                Image,
                GameObject.Transform.Position,
                null,
                Color,
                GameObject.Transform.Rotation,
                Origin,
                GameObject.Transform.Scale,
                SpriteEffectsHelper.GetEffects(FlipHorizontally, FlipVertically)
            );
        }
    }
}