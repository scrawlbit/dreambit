using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Elements.Components
{
    public class ImageRenderer : GameComponent
    {
        private readonly IContentDrawer _drawer;
        private Color _color;
        private Vector2 _origin;
        private bool _flipVertically;
        private bool _flipHorizontally;
        private IImage _image;

        public ImageRenderer(IContentDrawer drawer)
        {
            _drawer = drawer;

            Color = Color.White;
            Origin = Vector2.One / 2;
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
        public IImage Image
        {
            get => _image;
            set => Set(ref _image, value);
        }

        protected internal override void Draw()
        {
            if (Image == null)
                return;

            _drawer.Draw(
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
