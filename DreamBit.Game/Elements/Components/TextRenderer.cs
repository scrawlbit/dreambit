using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.Helpers;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Elements.Components
{
    public class TextRenderer : GameComponent
    {
        private readonly IContentDrawer _drawer;
        private string _text;
        private Color _color;
        private Vector2 _origin;
        private bool _flipVertically;
        private bool _flipHorizontally;
        private IFont _font;
        private Vector2 _size;

        public TextRenderer(IContentDrawer drawer)
        {
            _drawer = drawer;

            Color = Color.Black;
            Origin = Vector2.One / 2;
        }

        public override string Name => "Text Renderer";
        public string Text
        {
            get => _text;
            set
            {
                value = value.IfEmptyThenNull();

                if (Set(ref _text, value))
                    UpdateSize();
            }
        }
        public Color Color
        {
            get => _color;
            set => Set(ref _color, value);
        }
        public Vector2 Origin
        {
            get => _origin;
            set => Set(ref _origin, value);
        }
        public bool FlipVertically
        {
            get => _flipVertically;
            set => Set(ref _flipVertically, value);
        }
        public bool FlipHorizontally
        {
            get => _flipHorizontally;
            set => Set(ref _flipHorizontally, value);
        }
        public IFont Font
        {
            get => _font;
            set
            {
                if (Set(ref _font, value))
                    UpdateSize();
            }
        }
        public Vector2 Size
        {
            get => _size;
            private set => Set(ref _size, value);
        }

        internal Rectangle Area()
        {
            if (Text == null || Font?.SpriteFont == null)
                return Rectangle.Empty;

            return new Rectangle((Size * -Origin).ToPoint(), Size.ToPoint());
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

        private void UpdateSize()
        {
            Size = Font?.MeasureString(Text ?? "") ?? Vector2.Zero;
        }
    }
}
