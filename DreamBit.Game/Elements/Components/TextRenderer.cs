using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Elements.Components
{
    public class TextRenderer : GameComponent
    {
        private readonly IContentDrawer _drawer;
        private string _text;

        public TextRenderer(IContentDrawer drawer)
        {
            _drawer = drawer;

            Color = Color.Black;
            Origin = Vector2.One / 2;
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
        public Vector2 Origin { get; set; }
        public bool FlipVertically { get; set; }
        public bool FlipHorizontally { get; set; }
        public IFont Font { get; set; }
        public Vector2 Size
        {
            get => Font?.MeasureString(Text ?? "") ?? Vector2.Zero;
        }

        protected internal override void Draw()
        {
            if (Font == null || Text == null)
                return;

            _drawer.DrawString(
                Font,
                Text,
                GameObject.Transform.Position,
                Color,
                GameObject.Transform.Rotation,
                Size * Origin,
                GameObject.Transform.Scale,
                SpriteEffectsHelper.GetEffects(FlipHorizontally, FlipVertically)
            );
        }
    }
}
