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
    /// Equivale aos AudioBus do Godot / AudioMixerGroup do Unity.
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

        /// <summary>Volume próprio de um bus (não inclui o Master). Bus desconhecido = 1.</summary>
        public static float GetVolume(string bus)
            => _volumes.TryGetValue(bus ?? Master, out var v) ? v : 1f;

        /// <summary>Define o volume próprio de um bus (cria o bus se novo).</summary>
        public static void SetVolume(string bus, float volume)
        {
            if (!string.IsNullOrEmpty(bus))
                _volumes[bus] = MathHelper.Clamp(volume, 0f, 1f);
        }

        /// <summary>Volume efetivo de um bus: ele próprio × Master (o Master é × 1 de si).</summary>
        public static float Effective(string bus)
        {
            float master = _volumes.TryGetValue(Master, out var m) ? m : 1f;
            if (string.Equals(bus, Master, StringComparison.OrdinalIgnoreCase))
                return master;
            return GetVolume(bus) * master;
        }

        /// <summary>Nomes dos buses registrados (para UI).</summary>
        public static IReadOnlyCollection<string> Buses => _volumes.Keys;

        /// <summary>Restaura todos os buses para 1 (usado em testes/reset).</summary>
        public static void Reset()
        {
            _volumes.Clear();
            _volumes[Master] = 1f; _volumes[Music] = 1f; _volumes[Sfx] = 1f;
        }
    }
}
