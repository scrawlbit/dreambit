using System.Linq;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Pisca o sprite do objeto ao levar dano (feedback de acerto). Escuta o <see cref="Health"/>
    /// do mesmo objeto e tinge o <see cref="SpriteAnimator"/>/<see cref="SpriteRenderer"/> com a
    /// cor de flash por um instante. Sem Health, pode ser disparado por código (<see cref="Flash"/>).
    /// </summary>
    public sealed class SpriteFlash : SceneComponent
    {
        private float _duration = 0.1f;
        private float _timer;
        private Health? _health;
        private Color _originalSpriteColor = Color.White;
        private bool _cachedOriginal;

        public override string DisplayName => "Sprite Flash";

        /// <summary>Cor do flash.</summary>
        public Color FlashColor { get; set; } = Color.White;

        /// <summary>Duração do flash (s).</summary>
        public float Duration { get => _duration; set => Set(ref _duration, System.Math.Max(0.01f, value)); }

        /// <summary>Dispara o flash agora.</summary>
        public void Flash() => _timer = _duration;

        protected internal override void OnPlayStarted()
        {
            _timer = 0f;
            _cachedOriginal = false;
            _health = Owner?.Components.OfType<Health>().FirstOrDefault();
            if (_health != null)
                _health.Damaged += OnDamaged;
        }

        private void OnDamaged(float amount) => Flash();

        protected internal override void Update(GameTime gameTime)
        {
            var animator = Owner?.Components.OfType<SpriteAnimator>().FirstOrDefault();
            var sprite = Owner?.Components.OfType<SpriteRenderer>().FirstOrDefault();

            if (sprite != null && !_cachedOriginal)
            {
                _originalSpriteColor = sprite.Color;
                _cachedOriginal = true;
            }

            bool flashing = _timer > 0f;
            if (flashing)
                _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (animator != null)
                animator.Tint = flashing ? FlashColor : Color.White;
            if (sprite != null)
                sprite.Color = flashing ? FlashColor : _originalSpriteColor;
        }
    }
}
