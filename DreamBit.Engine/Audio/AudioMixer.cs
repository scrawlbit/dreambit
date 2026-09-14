using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Audio
{
    /// <summary>
    /// Mixer de áudio com barramentos (buses) nomeados — ex.: "Master", "Music", "SFX". Cada
    /// bus tem um volume [0..1]; o volume efetivo de um som é o do seu bus vezes o do "Master".
    /// Permite baixar toda a música sem mexer nos efeitos, ou mutar tudo pelo Master. Os
    /// <see cref="Components.AudioSource"/> leem o volume efetivo do seu bus ao vivo.
    /// </summary>
    public static class AudioMixer
    {
        public const string Master = "Master";
        public const string Music = "Music";
        public const string Sfx = "SFX";

        private static readonly Dictionary<string, float> _volumes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [Master] = 1f, [Music] = 1f, [Sfx] = 1f
            };

        // Ducking: multiplicador temporário por bus que recupera ao longo do tempo.
        private sealed class DuckState { public float Factor; public float Time; public float Duration; }
        private static readonly Dictionary<string, DuckState> _ducks = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Volume próprio de um bus (não inclui o Master). Bus desconhecido = 1.</summary>
        public static float GetVolume(string bus)
            => _volumes.TryGetValue(bus ?? Master, out var v) ? v : 1f;

        /// <summary>Define o volume próprio de um bus (cria o bus se novo).</summary>
        public static void SetVolume(string bus, float volume)
        {
            if (!string.IsNullOrEmpty(bus))
                _volumes[bus] = MathHelper.Clamp(volume, 0f, 1f);
        }

        /// <summary>Volume efetivo de um bus: ele próprio × Master × ducking atual.</summary>
        public static float Effective(string bus)
        {
            float master = _volumes.TryGetValue(Master, out var m) ? m : 1f;
            float duck = DuckFactor(bus);
            if (string.Equals(bus, Master, StringComparison.OrdinalIgnoreCase))
                return master * duck;
            return GetVolume(bus) * master * duck;
        }

        private static float DuckFactor(string bus)
        {
            if (bus != null && _ducks.TryGetValue(bus, out var d) && d.Duration > 0f)
            {
                // Recupera linearmente do fator até 1 ao longo da duração.
                float k = MathHelper.Clamp(d.Time / d.Duration, 0f, 1f);
                return MathHelper.Lerp(d.Factor, 1f, k);
            }
            return 1f;
        }

        /// <summary>Abaixa temporariamente um bus (ex.: a música quando toca um diálogo/efeito),
        /// recuperando o volume ao longo de <paramref name="duration"/> s. Chame ao tocar o efeito.</summary>
        public static void Duck(string bus, float factor, float duration)
        {
            if (string.IsNullOrEmpty(bus) || duration <= 0f)
                return;
            _ducks[bus] = new DuckState { Factor = MathHelper.Clamp(factor, 0f, 1f), Time = 0f, Duration = duration };
        }

        /// <summary>Avança o ducking (o host chama por frame; a cena chama no Update).</summary>
        public static void Tick(float dt)
        {
            foreach (var d in _ducks.Values)
                if (d.Duration > 0f)
                    d.Time += dt;
        }

        /// <summary>Nomes dos buses registrados (para UI).</summary>
        public static IReadOnlyCollection<string> Buses => _volumes.Keys;

        /// <summary>Restaura todos os buses para 1 (usado em testes/reset).</summary>
        public static void Reset()
        {
            _volumes.Clear();
            _volumes[Master] = 1f; _volumes[Music] = 1f; _volumes[Sfx] = 1f;
            _ducks.Clear();
        }
    }
}
