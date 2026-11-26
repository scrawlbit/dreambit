using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Audio
{
    /// <summary>
    /// Toca efeitos sonoros "soltos" (sem componente na cena): carrega o WAV pelo <see cref="SoundCache"/> e toca
    /// no volume efetivo de um bus do <see cref="AudioMixer"/> (padrão "SFX", vezes o "Master"). Para pulos,
    /// golpes, menus... Sem dispositivo de áudio, sem arquivo ou com volume zero, simplesmente não toca.
    /// </summary>
    public static class SoundPlayer
    {
        private static readonly Random Rng = new(7);
        private static readonly Dictionary<string, double> LastPlayed = new(StringComparer.OrdinalIgnoreCase);
        private static double _clock;

        /// <summary>Liga/desliga todos os efeitos tocados por aqui (testes, ferramentas sem áudio).</summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>Volume final de um efeito: o próprio × o volume efetivo do bus (que já inclui o Master).</summary>
        public static float EffectiveVolume(float volume, string bus = AudioMixer.Sfx)
            => MathHelper.Clamp(volume, 0f, 1f) * AudioMixer.Effective(bus);

        /// <summary>Avança o relógio usado pelo intervalo mínimo entre repetições (chame uma vez por frame).</summary>
        public static void Tick(float dt) => _clock += dt;

        /// <summary>Esquece os intervalos (troca de cena, testes).</summary>
        public static void Reset() { LastPlayed.Clear(); _clock = 0; }

        /// <summary>Se o mesmo som pode tocar agora (não tocou há menos de <paramref name="minGap"/> s); marca se puder.</summary>
        public static bool TryClaim(string path, float minGap)
        {
            if (minGap > 0f && LastPlayed.TryGetValue(path, out var last) && _clock - last < minGap) return false;
            LastPlayed[path] = _clock;
            return true;
        }

        /// <summary>
        /// Toca um WAV por caminho (relativo às raízes de assets). <paramref name="pitchVariance"/> sorteia a altura em
        /// ±variância (oitavas, -1..1) para não cansar; <paramref name="minGap"/> evita empilhar o mesmo som no mesmo
        /// instante. Devolve true se tocou.
        /// </summary>
        public static bool Play(string? path, float volume = 1f, string bus = AudioMixer.Sfx, float pitchVariance = 0f,
            float pan = 0f, float minGap = 0f)
        {
            if (!Enabled || string.IsNullOrEmpty(path)) return false;
            float v = EffectiveVolume(volume, bus);
            if (v <= 0.001f || !TryClaim(path, minGap)) return false;
            try
            {
                var sound = SoundCache.Get(path);
                if (sound == null) return false;
                float pitch = pitchVariance > 0f ? (float)(Rng.NextDouble() * 2 - 1) * pitchVariance : 0f;
                return sound.Play(v, MathHelper.Clamp(pitch, -1f, 1f), MathHelper.Clamp(pan, -1f, 1f));
            }
            catch (Exception)
            {
                return false; // sem dispositivo de áudio (máquina sem som, testes, editor)
            }
        }
    }
}
