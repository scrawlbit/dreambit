using System.Collections.Generic;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Movimento top-down (8 direções / twin-stick), sem gravidade, com colisão sólida contra
    /// tilemap e <see cref="BoxCollider"/>. Para jogos de cima (RPG, dungeon, tile). Complementa
    /// o <see cref="PlatformerController"/> (plataforma) e o <see cref="Rigidbody2D"/> (física).
    /// </summary>
    public sealed class TopDownController : SceneComponent
    {
        private float _moveSpeed = 200f;
        private bool _useKeyboard = true;
        private float _halfWidth = 16f;
        private float _halfHeight = 16f;

        private Vector2 _velocity;

        public override string DisplayName => "Top-Down Controller";

        public float MoveSpeed { get => _moveSpeed; set => Set(ref _moveSpeed, value); }
        public bool UseKeyboard { get => _useKeyboard; set => Set(ref _useKeyboard, value); }
        public float HalfWidth { get => _halfWidth; set => Set(ref _halfWidth, System.Math.Max(0f, value)); }
        public float HalfHeight { get => _halfHeight; set => Set(ref _halfHeight, System.Math.Max(0f, value)); }

        /// <summary>Velocidade atual (px/s) neste frame.</summary>
        public Vector2 CurrentVelocity => _velocity;

        /// <summary>True se está se movendo.</summary>
        public bool IsMoving => _velocity.LengthSquared() > 1f;

        /// <summary>Direção de movimento definida por código (quando não é por teclado), em [-1,1].</summary>
        public Vector2 MoveInput { get; set; }

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
                return;

            var dir = _useKeyboard ? new Vector2(GameInput.Horizontal(), GameInput.Vertical()) : MoveInput;
            if (dir.LengthSquared() > 1f)      // normaliza para a diagonal não ser mais rápida
                dir.Normalize();

            _velocity = dir * _moveSpeed;

            var position = Owner.Transform.Position;
            var solids = CollectSolids(Owner.Scene);

            float x = SolidPhysics.ResolveX(solids, position.X, position.X + _velocity.X * dt, position.Y, _halfWidth, _halfHeight);
            var (y, _, _) = SolidPhysics.ResolveY(solids, x, position.Y, position.Y + _velocity.Y * dt, _halfWidth, _halfHeight);

            Owner.Transform.Position = new Vector2(x, y);
        }

        private List<SolidPhysics.Box> CollectSolids(Scene? scene)
        {
            var boxes = new List<SolidPhysics.Box>();
            if (scene == null)
                return boxes;

            foreach (var obj in AllObjects(scene.Objects))
            {
                if (obj == Owner)
                    continue;
                foreach (var component in obj.Components)
                {
                    if (component is BoxCollider collider)
                    {
                        var (min, max) = collider.WorldBounds();
                        boxes.Add(new SolidPhysics.Box(min, max));
                    }
                    else if (component is TilemapRenderer tilemap)
                    {
                        foreach (var (min, max) in tilemap.SolidBoxes())
                            boxes.Add(new SolidPhysics.Box(min, max));
                    }
                }
            }
            return boxes;
        }

        private static IEnumerable<GameObject> AllObjects(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in AllObjects(obj.Children))
                    yield return child;
            }
        }
    }
}
