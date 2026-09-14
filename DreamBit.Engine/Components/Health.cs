using System;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Vida de um objeto: recebe dano, cura, e dispara eventos/mensagens ao ser ferido ou morrer.
    /// Base de combate — combine com <see cref="Hurtbox"/> (recebe golpes) e <see cref="Hitbox"/>
    /// (dá golpes). A <see cref="Fraction"/> alimenta uma barra de vida de HUD.
    /// </summary>
    public sealed class Health : SceneComponent
    {
        private float _max = 100f;
        private float _invulnTime = 0.2f;
        private float _current;
        private float _invuln;

        public override string DisplayName => "Health";

        /// <summary>Vida máxima.</summary>
        public float Max { get => _max; set => Set(ref _max, MathHelper.Max(1f, value)); }

        /// <summary>Tempo de invulnerabilidade após levar dano (i-frames), em segundos.</summary>
        public float InvulnTime { get => _invulnTime; set => Set(ref _invulnTime, MathHelper.Max(0f, value)); }

        /// <summary>Mensagem enviada ao levar dano (opcional).</summary>
        public string SendOnHit { get; set; } = string.Empty;

        /// <summary>Mensagem enviada ao morrer (opcional).</summary>
        public string SendOnDeath { get; set; } = "death";

        /// <summary>Remove o objeto da cena ao morrer.</summary>
        public bool DestroyOnDeath { get; set; }

        /// <summary>Vida atual.</summary>
        public float Current => _current;

        /// <summary>Vida em [0,1] (para barras de HUD).</summary>
        public float Fraction => _max > 0f ? MathHelper.Clamp(_current / _max, 0f, 1f) : 0f;

        public bool IsDead => _current <= 0f;

        /// <summary>Está invulnerável neste instante (logo após um golpe).</summary>
        public bool Invulnerable => _invuln > 0f;

        /// <summary>Disparado ao levar dano (passa a quantidade aplicada).</summary>
        public event Action<float>? Damaged;

        /// <summary>Disparado ao morrer.</summary>
        public event Action? Died;

        protected internal override void OnPlayStarted()
        {
            _current = _max;
            _invuln = 0f;
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (_invuln > 0f)
                _invuln -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>Aplica dano (ignorado se morto ou invulnerável). Dispara eventos/mensagens.</summary>
        public void Damage(float amount)
        {
            if (IsDead || _invuln > 0f || amount <= 0f)
                return;

            _current = MathHelper.Max(0f, _current - amount);
            _invuln = _invulnTime;
            Damaged?.Invoke(amount);
            if (!string.IsNullOrEmpty(SendOnHit))
                Owner?.Scene?.Send(SendOnHit, Owner);

            if (_current <= 0f)
            {
                Died?.Invoke();
                if (!string.IsNullOrEmpty(SendOnDeath))
                    Owner?.Scene?.Send(SendOnDeath, Owner);
                if (DestroyOnDeath && Owner != null)
                    Owner.Scene?.Destroy(Owner);
            }
        }

        /// <summary>Cura (limitado ao máximo). Não ressuscita se já morto.</summary>
        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;
            _current = MathHelper.Min(_max, _current + amount);
        }
    }
}
