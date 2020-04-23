using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Elements.Components
{
    public class Camera : GameComponent
    {
        private readonly IContentDrawer _drawer;
        private bool _active;

        public Camera(IContentDrawer drawer)
        {
            _drawer = drawer;
        }

        public override string Name => "Camera";
        private Vector2 Size => new Vector2(800, 600);
        public bool IsActive
        {
            get => _active;
            set => Set(ref _active, value);
        }

        protected internal override void Draw()
        {
            const int size = 40;
            const int thickness = 5;
            var halfSize = Size * 0.5f;

            var topLeft = GameObject.Transform.Position - halfSize;
            var topRight = GameObject.Transform.Position + halfSize * new Vector2(1, -1);
            var bottomLeft = GameObject.Transform.Position + halfSize * new Vector2(-1, 1);
            var bottomRight = GameObject.Transform.Position + halfSize;

            _drawer.DrawLine(topLeft, size, 0, Color.White, thickness);
            _drawer.DrawLine(topLeft, size, MathHelper.PiOver2, Color.White, thickness);

            _drawer.DrawLine(topRight, size, MathHelper.Pi, Color.White, thickness);
            _drawer.DrawLine(topRight, size, MathHelper.PiOver2, Color.White, thickness);

            _drawer.DrawLine(bottomLeft, size, -MathHelper.PiOver2, Color.White, thickness);
            _drawer.DrawLine(bottomLeft, size, 0, Color.White, thickness);

            _drawer.DrawLine(bottomRight, size, -MathHelper.PiOver2, Color.White, thickness);
            _drawer.DrawLine(bottomRight, size, MathHelper.Pi, Color.White, thickness);
        }
    }
}
