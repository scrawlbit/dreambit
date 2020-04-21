using Microsoft.Xna.Framework;

namespace DreamBit.Game.Elements
{
    public interface ITransformationValues
    {
        Vector2 Position { get; set; }
        float Rotation { get; set; }
        Vector2 Scale { get; set; }

        Matrix Matrix { get; }
    }
}
