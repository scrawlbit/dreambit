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
        private Vector2 _origin;
        private Vector2? _offset;

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
        public Vector2 Origin
        {
            get => _origin;
            set
            {
                _origin = value;
                _offset = null;
            }
        }
        public bool FlipVertically { get; set; }
        public bool FlipHorizontally { get; set; }
        public IFont Font { get; set; }

        protected internal override void Draw()
        {
            if (Font == null || Text == null)
                return;

            if (_offset == null)
                _offset = Font.MeasureString(Text) * Origin;

            _drawer.DrawString(
                Font,
                Text,
                GameObject.Transform.Position,
                Color,
                GameObject.Transform.Rotation,
                _offset,
                GameObject.Transform.Scale,
                SpriteEffectsHelper.GetEffects(FlipHorizontally, FlipVertically)
            );
        }
    }
}
