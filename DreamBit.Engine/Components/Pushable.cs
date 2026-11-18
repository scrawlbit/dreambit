using System;
using System.Linq;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Objeto empurrável (caixote, pedra): precisa de um <see cref="BoxCollider"/>. O
    /// PlatformerController com a habilidade Push o empurra; o jogo pode puxar chamando
    /// <see cref="TryMove"/>. Cai com gravidade até apoiar num sólido ou ledge.
    /// </summary>
    [GameComponent("DreamBit/Pushable", Id = "dreambit.pushable")]
    public sealed class Pushable : SceneComponent
    {
        private float _vy;

        public override string DisplayName => "Pushable";

        /// <summary>Gravidade (px/s²); 0 = não cai.</summary>
        public float Gravity { get; set; } = 1400f;

        /// <summary>Peso: fração do movimento aceita (1 = leve, 0.3 = pesado).</summary>
        [Range(0.05f, 1f)]
        public float Weight { get; set; } = 1f;

        /// <summary>True se está apoiado.</summary>
        public bool Grounded { get; private set; }

        /// <summary>Move na horizontal respeitando os sólidos; devolve quanto andou de fato.</summary>
        public float TryMove(float dx)
        {
            var col = Owner.Components.OfType<BoxCollider>().FirstOrDefault();
            if (col == null || dx == 0f)
                return 0f;
            dx *= Math.Clamp(Weight, 0.05f, 1f);
            var solids = SolidQuery.Collect(Owner.Scene, Owner, out _);
            var center = Owner.Transform.WorldPosition + col.Offset;
            var half = col.Size / 2f;
            float to = SolidPhysics.ResolveX(solids, center.X, center.X + dx, center.Y, half.X, half.Y - 0.5f);
            float moved = to - center.X;
            Owner.Transform.Position += new Vector2(moved, 0f);
            return moved;
        }

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var col = Owner.Components.OfType<BoxCollider>().FirstOrDefault();
            if (dt <= 0f || col == null || Gravity <= 0f)
                return;

            var solids = SolidQuery.Collect(Owner.Scene, Owner, out _);
            var center = Owner.Transform.WorldPosition + col.Offset;
            var half = col.Size / 2f;
            _vy = Math.Min(_vy + Gravity * dt, 1600f);
            float toY = center.Y + _vy * dt;
            var (y, grounded, _) = SolidPhysics.ResolveY(solids, center.X, center.Y, toY, half.X - 0.5f, half.Y);
            if (!grounded && Owner.Scene != null)
            {
                float surface = LedgePhysics.FindLanding(Owner.Scene.Ledges, center.X, center.Y + half.Y, y + half.Y);
                if (!float.IsNaN(surface)) { y = surface - half.Y; grounded = true; }
            }
            if (grounded) _vy = 0f;
            Grounded = grounded;
            Owner.Transform.Position += new Vector2(0f, y - center.Y);
        }
    }
}
