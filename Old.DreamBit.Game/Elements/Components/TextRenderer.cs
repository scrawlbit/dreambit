using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using DreamBit.Game.Reading.Attributes;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Elements.Components
{
    public interface ITextRenderer
    {
        string Text { get; set; }
        Color Color { get; set; }
        bool FlipVertically { get; set; }
        bool FlipHorizontally { get; set; }
        Vector2 Origin { get; set; }
        IFont Font { get; set; }
    }

    internal class TextRenderer : GameObjectComponent, ITextRenderer
    {
        private readonly IDrawBatch _drawBatch;
        private string _text;

        internal TextRenderer(IDrawBatch drawBatch)
        {
            _drawBatch = drawBatch;
        }

        public string Text
        {
            get => _text;
            set
            {
                value = value?.Trim();
                value = value != "" ? value : null;

                _text = value;
            }
        }
        public Color Color { get; set; }
        public bool FlipVertically { get; set; }
        public bool FlipHorizontally { get; set; }
        public Vector2 Origin { get; set; }
        [ContentReference("FileId")]
        public IFont Font { get; set; }
        
        protected internal override void Draw()
        {
            if (Font == null || Text == null)
                return;

            _drawBatch.DrawString(
                Font,
                Text,
                GameObject.Transform.Position,
                Color,
                GameObject.Transform.Rotation,
                Origin,
                GameObject.Transform.Scale,
                SpriteEffectsHelper.GetEffects(FlipHorizontally, FlipVertically)
            );
        }
    }
}