using DreamBit.Pipeline.Files;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Elements.Components
{
    public class ImageRenderer : GameObjectComponent
    {
        public ImageRenderer()
        {
            Origin = Vector2.One / 2;
        }

        public Color Color { get; set; }
        public bool FlipVertically { get; set; }
        public bool FlipHorizontally { get; set; }
        public Vector2 Origin { get; set; }
        public PipelineImage Image { get; set; }
    }
}
