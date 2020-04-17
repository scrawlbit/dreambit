using Microsoft.Xna.Framework;

namespace DreamBit.Extension.Module
{
    public class SelectionData
    {
        public bool IsVisible { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public Matrix Matrix { get; set; }
    }
}
