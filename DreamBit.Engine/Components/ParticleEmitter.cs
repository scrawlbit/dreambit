using System;
using System.Collections.Generic;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Emissor de partículas simples: no play mode lança partículas para cima (com
    /// espalhamento), sob gravidade, que desaparecem ao fim da vida. (Marco 3.)
    /// </summary>
    public sealed class ParticleEmitter : SceneComponent
    {
        private float _emitRate = 30f;
        private float _lifetime = 1.4f;
        private float _speed = 140f;
        private float _spread = 0.6f;
        private float _size = 8f;
        private float _endSize = 8f;
        private float _gravityY = 220f;
        private Color _color = new(255, 180, 90);
        private Color _endColor = new(255, 180, 90);
        private int _burstCount;
        private bool _emitOnStart;

        private readonly List<Particle> _particles = new();
        private readonly Random _rng = new();
        private double _accumulator;

        public override string DisplayName => "Particle Emitter";

        public float EmitRate { get => _emitRate; set => Set(ref _emitRate, Math.Max(0f, value)); }
        public float Lifetime { get => _lifetime; set => Set(ref _lifetime, Math.Max(0.05f, value)); }
        public float Speed { get => _speed; set => Set(ref _speed, value); }
        public float Spread { get => _spread; set => Set(ref _spread, value); }
        public float Size { get => _size; set => Set(ref _size, Math.Max(1f, value)); }
        /// <summary>Tamanho ao fim da vida (interpola de <see cref="Size"/> até aqui).</summary>
        public float EndSize { get => _endSize; set => Set(ref _endSize, Math.Max(0f, value)); }
        public float GravityY { get => _gravityY; set => Set(ref _gravityY, value); }
        public Color Color { get => _color; set => Set(ref _color, value); }
        /// <summary>Cor ao fim da vida (interpola de <see cref="Color"/> até aqui).</summary>
        public Color EndColor { get => _endColor; set => Set(ref _endColor, value); }
        /// <summary>Quantidade emitida de uma vez num burst (explosão). 0 = sem burst.</summary>
        public int BurstCount { get => _burstCount; set => Set(ref _burstCount, Math.Max(0, value)); }
        /// <summary>Emite o burst ao iniciar (efeito one-shot, ex.: explosão).</summary>
        public bool EmitOnStart { get => _emitOnStart; set => Set(ref _emitOnStart, value); }

        /// <summary>Partículas vivas neste instante (para depurar/testar).</summary>
        public int ActiveParticles => _particles.Count;

        /// <summary>Emite <paramref name="count"/> partículas de uma vez (burst).</summary>
        public void Burst(int count)
        {
            for (int i = 0; i < count; i++)
                Spawn();
        }

        protected internal override void OnPlayStarted()
        {
            _particles.Clear();
            if (_emitOnStart && _burstCount > 0)
                Burst(_burstCount);
        }

        protected internal override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt <= 0f)
                return;

            if (_emitRate > 0f)
            {
                _accumulator += _emitRate * dt;
                while (_accumulator >= 1.0)
                {
                    _accumulator -= 1.0;
                    Spawn();
                }
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Velocity.Y += _gravityY * dt;
                p.Position += p.Velocity * dt;
                p.Life -= dt;

                if (p.Life <= 0f)
                    _particles.RemoveAt(i);
                else
                    _particles[i] = p;
            }
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            foreach (var p in _particles)
            {
                float alpha = MathHelper.Clamp(p.Life / _lifetime, 0f, 1f);
                float age = 1f - alpha; // 0 = novo, 1 = fim da vida
                var color = Color.Lerp(_color, _endColor, age) * alpha;
                float size = MathHelper.Lerp(_size, _endSize, age);
                var world = Matrix.CreateTranslation(p.Position.X, p.Position.Y, 0f);
                drawing.DrawQuad(world, new Vector2(size), color);
            }
        }

        private void Spawn()
        {
            float baseAngle = -MathHelper.PiOver2; // para cima
            float angle = baseAngle + ((float)_rng.NextDouble() * 2f - 1f) * _spread;
            var velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * _speed;

            _particles.Add(new Particle
            {
                Position = Owner.Transform.WorldPosition,
                Velocity = velocity,
                Life = _lifetime
            });
        }

        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;
        }
    }
}
