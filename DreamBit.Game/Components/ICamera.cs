using Microsoft.Xna.Framework;

namespace DreamBit.Game.Components
{
    internal interface ICamera
    {
        bool IsActive { get; set; }
        Vector2 Size { get; }
        Vector2 Position { get; }
        float Rotation { get; }
        Vector2 Zoom { get; }
    }
}