﻿using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;

namespace DreamBit.Game.Elements.Components
{
    public class ImageRenderer : GameComponent
    {
        private readonly IContentDrawer _drawer;
        private Vector2 _origin;
        private Vector2? _offset;

        public ImageRenderer(IContentDrawer drawer)
        {
            _drawer = drawer;

            Color = Color.White;
            Origin = Vector2.One / 2;
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
        public IImage Image { get; set; }

        protected internal override void Draw()
        {
            if (Image == null)
                return;

            if (_offset == null)
                _offset = new Vector2(Image.Width, Image.Height) * Origin;

            _drawer.Draw(
                Image,
                GameObject.Transform.Position,
                null,
                Color,
                GameObject.Transform.Rotation,
                _offset,
                GameObject.Transform.Scale,
                SpriteEffectsHelper.GetEffects(FlipHorizontally, FlipVertically)
            );
        }
    }
}
