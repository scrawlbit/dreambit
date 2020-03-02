using DreamBit.Game.Components;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Tests.Mocks.Components
{
    public class CameraMock : ICamera
    {
        public bool IsActive { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Zoom { get; set; }
    }
}