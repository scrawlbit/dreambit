using System;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Controlador de plataforma: no play mode aplica gravidade e faz o objeto
    /// pousar sobre as ledges da cena (uma velocidade horizontal opcional o faz
    /// "andar" acompanhando a superfície). Fecha o laço das ledges. (Marco 3.)
    /// </summary>
    public sealed class PlatformerController : SceneComponent
    {
        private float _gravity = 1400f;
        private float _halfHeight = 24f;
        private float _horizontalSpeed = 0f;
        private float _velocityY;

        public override string DisplayName => "Platformer Controller";

        /// <summary>Aceleração da gravidade (px/s²).</summary>
        public float Gravity { get => _gravity; set => Set(ref _gravity, value); }

        /// <summary>Distância do centro até os "pés" do objeto.</summary>
        public float HalfHeight { get => _halfHeight; set => Set(ref _halfHeight, Math.Max(0f, value)); }

        /// <summary>Velocidade horizontal (px/s); positiva anda para a direita.</summary>
        public float HorizontalSpeed { get => _horizontalSpeed; set => Set(ref _horizontalSpeed, value); }

        public void Reset() => _velocityY = 0f;

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
                return;

            var position = Owner.Transform.Position;
            float x = position.X + _horizontalSpeed * dt;

            _velocityY += _gravity * dt;
            float newY = position.Y + _velocityY * dt;

            var scene = Owner.Scene;
            if (scene != null)
            {
                float feetFrom = position.Y + _halfHeight;
                float feetTo = newY + _halfHeight;
                float surface = LedgePhysics.FindLanding(scene.Ledges, x, feetFrom, feetTo);

                if (!float.IsNaN(surface))
                {
                    newY = surface - _halfHeight;
                    _velocityY = 0f;
                }
            }

            Owner.Transform.Position = new Vector2(x, newY);
        }
    }
}
