using Microsoft.Xna.Framework;

namespace DreamBit.Game.Components
{
    public interface ISceneCamera
    {
        Vector2 Size { get; set; }
        Vector2 Position { get; set; }
        float Rotation { get; set; }
        Vector2 Zoom { get; set; }

        Matrix TransformMatrix { get; }
    }
}