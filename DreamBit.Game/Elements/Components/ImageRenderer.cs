using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Elements.Components
{
    public class ImageRenderer : GameComponent
    {
        private readonly IContentDrawer _drawer;

        public ImageRenderer(IContentDrawer drawer)
        {
            _drawer = drawer;

            Color = Color.White;
            Origin = Vector2.One / 2;
        }

        public Color Color { get; set; }
        public Vector2 Origin { get; set; }
        public bool FlipVertically { get; set; }
        public bool FlipHorizontally { get; set; }
        public Image Image { get; set; }

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
                Origin,
                GameObject.Transform.Scale,
                SpriteEffectsHelper.GetEffects(FlipHorizontally, FlipVertically)
            );
        }
    }
}
