using System;
using System.Linq;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Objeto quebrável (parede rachada, piso frágil, caixote, vinhas que queimam). Quebra
    /// quando atingido por um dos tipos em <see cref="BreakBy"/>: o PlatformerController envia
    /// "Dash" e "GroundPound"; o jogo pode enviar outros ("Fire", "Attack"…) com
    /// <see cref="TryBreak"/>. Com <see cref="PersistKey"/>, fica quebrado para sempre (save).
    /// </summary>
    [GameComponent("DreamBit/Breakable", Id = "dreambit.breakable")]
    public sealed class Breakable : SceneComponent
    {
        public const string ByDash = "Dash";
        public const string ByGroundPound = "GroundPound";
        public const string ByAttack = "Attack";

        private int _hits;

        public override string DisplayName => "Breakable";

        /// <summary>Tipos de golpe que quebram, separados por vírgula (ex.: "Dash,Fire").</summary>
        [Tooltip("Tipos que quebram, separados por vírgula: Dash, GroundPound, Attack ou do jogo (Fire...)")]
        public string BreakBy { get; set; } = ByDash;

        /// <summary>Golpes necessários.</summary>
        [Range(1, 99)]
        public int HitsToBreak { get; set; } = 1;

        /// <summary>Mensagem enviada à cena ao quebrar (vazio = nenhuma).</summary>
        public string SendOnBreak { get; set; } = "broken";

        /// <summary>Remove o objeto ao quebrar (com partículas, só desliga colisor e visual).</summary>
        public bool DestroyOnBreak { get; set; } = true;

        /// <summary>Partículas (do ParticleEmitter do objeto) emitidas ao quebrar.</summary>
        public int BurstParticles { get; set; } = 24;

        /// <summary>Chave de persistência no WorldState (vazio = volta a existir ao recarregar a cena).</summary>
        [Tooltip("Se definido, o objeto continua quebrado depois de salvar/recarregar (WorldState)")]
        public string PersistKey { get; set; } = "";

        public bool IsBroken { get; private set; }

        /// <summary>Disparado ao quebrar (com quem quebrou, se conhecido).</summary>
        public event Action<Breakable, GameObject?>? Broken;

        public bool Accepts(string kind)
            => BreakBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(k => string.Equals(k, kind, StringComparison.OrdinalIgnoreCase));

        /// <summary>Aplica um golpe do tipo <paramref name="kind"/>; true se quebrou agora.</summary>
        public bool Hit(string kind, GameObject? by = null)
        {
            if (IsBroken || !Enabled || !Accepts(kind))
                return false;
            if (++_hits < Math.Max(1, HitsToBreak))
                return false;
            Break(by);
            return true;
        }

        /// <summary>Quebra imediatamente.</summary>
        public void Break(GameObject? by = null)
        {
            if (IsBroken) return;
            IsBroken = true;
            if (!string.IsNullOrEmpty(PersistKey))
                Saving.WorldState.SetFlag(PersistKey);

            var particles = Owner.Components.OfType<ParticleEmitter>().FirstOrDefault();
            if (particles != null && BurstParticles > 0)
                particles.Burst(BurstParticles);
            Broken?.Invoke(this, by);
            if (!string.IsNullOrEmpty(SendOnBreak))
                Owner.Scene?.Send(SendOnBreak, Owner);

            if (DestroyOnBreak && (particles == null || BurstParticles <= 0))
                Owner.Scene?.Destroy(Owner);
            else
                Disable();
        }

        protected internal override void OnPlayStarted()
        {
            _hits = 0;
            IsBroken = false;
            if (!string.IsNullOrEmpty(PersistKey) && Saving.WorldState.HasFlag(PersistKey))
            {
                IsBroken = true;
                Disable();
            }
        }

        // Desliga colisão e visuais (mantém o objeto para as partículas terminarem).
        private void Disable()
        {
            foreach (var c in Owner.Components)
                if (c is BoxCollider || c is SpriteRenderer || c is SpriteAnimator || c is TilemapRenderer || c is Hurtbox)
                    c.Enabled = false;
        }

        /// <summary>Tenta quebrar <paramref name="target"/> com um golpe do tipo <paramref name="kind"/>.</summary>
        public static bool TryBreak(GameObject target, string kind, GameObject? by = null)
            => target.Components.OfType<Breakable>().FirstOrDefault()?.Hit(kind, by) ?? false;
    }
}
