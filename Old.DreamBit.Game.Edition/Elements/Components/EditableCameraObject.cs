using Microsoft.Xna.Framework;

namespace DreamBit.Game.Elements.Components
{
    public class EditableCameraObject : GameObjectComponent
    {
        internal EditableCameraObject()
        {
        }

        public ComponentType Type => ComponentType.Camera;
        public Vector2 Size { get; set; }
        public bool IsActive { get; set; }
    }
}