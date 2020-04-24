using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Elements.Components
{
    public class ImageRenderer : GameComponent
    {
        private Color _color;
        private Vector2 _origin;
        private bool _flipVertically;
        private bool _flipHorizontally;
        private IImage _image;

        public ImageRenderer()
        {
            Color = Color.White;
            Origin = Vector2.One / 2;
        }

        public override string Name => "Image Renderer";
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
        public IImage Image
        {
            get => _image;
            set => Set(ref _image, value);
        }

        internal Rectangle Area()
        {
            if (Image?.Texture == null)
                return Rectangle.Empty;

            return new Rectangle((Image.Size * -Origin).ToPoint(), Image.Size.ToPoint());
        }

        protected internal override void Draw(IContentDrawer drawer)
        {
            if (Image == null)
                return;

            drawer.Draw(
                Image,
                GameObject.Transform.Position,
                null,
                Color,
                GameObject.Transform.Rotation,
                Image.Size * Origin,
                GameObject.Transform.Scale,
                SpriteEffectsHelper.GetEffects(FlipHorizontally, FlipVertically)
            );
        }
    }
}
