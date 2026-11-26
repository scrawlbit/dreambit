using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DreamBit.Engine.Audio
{
    /// <summary>
    /// Música de fundo que não depende da cena: toca um WAV em loop no bus "Music" do <see cref="AudioMixer"/> (vezes o
    /// "Master"), troca de faixa com crossfade e para com fade. Sobrevive a troca de cenas (o jogo escolhe a faixa de
    /// cada sala e chama <see cref="Play"/>; pedir a faixa que já toca não reinicia). Uma faixa que ainda está sumindo
    /// volta do volume em que estava, sem recomeçar do zero (ex.: chefe → sala → chefe). Sem dispositivo de áudio, sem
    /// arquivo ou com <see cref="Enabled"/> desligado, a lógica (faixa atual e níveis de fade) segue igual, só não soa.
    /// Chame <see cref="Tick"/> uma vez por frame.
    /// </summary>
    public static class MusicPlayer
    {
        /// <summary>Uma faixa tocando (ou sumindo): nível do fade 0..1 e o volume próprio da faixa.</summary>
        public sealed class Voice
        {
            internal SoundEffectInstance? Instance;
            internal float From, To, Time, Duration;

            public string Path { get; internal set; } = "";
            /// <summary>Nível atual do fade (0..1).</summary>
            public float Level { get; internal set; }
            /// <summary>Volume próprio da faixa (0..1), vezes o nível e o bus.</summary>
            public float Volume { get; internal set; } = 1f;
            /// <summary>Indo para o silêncio (sai da lista ao chegar a 0).</summary>
            public bool FadingOut => To <= 0f;
            /// <summary>Se há som de verdade (arquivo carregado e dispositivo de áudio).</summary>
            public bool Audible => Instance != null;
        }

        private static readonly List<Voice> _voices = new();
        private static string _bus = AudioMixer.Music;

        /// <summary>Liga/desliga o som (testes, ferramentas): com false, nenhuma faixa é carregada, mas o estado segue.</summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>Bus do mixer da música (padrão "Music").</summary>
        public static string Bus { get => _bus; set => _bus = string.IsNullOrEmpty(value) ? AudioMixer.Music : value; }

        /// <summary>Faixa pedida por último (a que está entrando ou tocando), ou null se parada.</summary>
        public static string? Current { get; private set; }

        /// <summary>Faixas soando agora (a atual e as que ainda estão sumindo).</summary>
        public static IReadOnlyList<Voice> Voices => _voices;

        /// <summary>Volume final de uma faixa: nível do fade × volume da faixa × volume efetivo do bus (inclui o Master).</summary>
        public static float EffectiveVolume(float level, float volume = 1f)
            => MathHelper.Clamp(level, 0f, 1f) * MathHelper.Clamp(volume, 0f, 1f) * AudioMixer.Effective(_bus);

        /// <summary>
        /// Toca <paramref name="path"/> em loop, com crossfade de <paramref name="fade"/> s a partir do que estiver
        /// tocando. A mesma faixa não reinicia (só ajusta o volume). Null/vazio = <see cref="Stop"/>. Devolve true se a
        /// faixa soa (arquivo carregado e dispositivo de áudio).
        /// </summary>
        public static bool Play(string? path, float fade = 1.5f, float volume = 1f)
        {
            if (string.IsNullOrEmpty(path)) { Stop(fade); return false; }
            volume = MathHelper.Clamp(volume, 0f, 1f);
            Voice? voice = null;
            foreach (var v in _voices)
                if (string.Equals(v.Path, path, StringComparison.OrdinalIgnoreCase)) { voice = v; break; }

            foreach (var v in _voices)
                if (v != voice && !v.FadingOut) FadeVoice(v, 0f, fade);

            if (voice == null)
            {
                voice = new Voice { Path = path, Level = 0f, Volume = volume };
                voice.Instance = Start(path);
                _voices.Add(voice);
            }
            voice.Volume = volume;
            if (voice.To < 1f) FadeVoice(voice, 1f, fade);   // nova ou sumindo: entra; já entrando/tocando: segue
            Current = path;
            Apply(voice);
            return voice.Audible;
        }

        /// <summary>Some com a música em <paramref name="fade"/> s (0 = corta já).</summary>
        public static void Stop(float fade = 1f)
        {
            Current = null;
            foreach (var v in _voices)
                if (!v.FadingOut) FadeVoice(v, 0f, fade);
            if (fade <= 0f) Tick(0f);
        }

        /// <summary>Corta tudo na hora e esquece as faixas (troca de jogo, testes).</summary>
        public static void Reset()
        {
            foreach (var v in _voices) Silence(v);
            _voices.Clear();
            Current = null;
        }

        /// <summary>Avança os fades e reflete ao vivo o volume do bus/Master (chame uma vez por frame).</summary>
        public static void Tick(float dt)
        {
            for (int i = _voices.Count - 1; i >= 0; i--)
            {
                var v = _voices[i];
                if (v.Duration > 0f)
                {
                    v.Time += Math.Max(0f, dt);
                    float k = MathHelper.Clamp(v.Time / v.Duration, 0f, 1f);
                    v.Level = MathHelper.Lerp(v.From, v.To, k);
                    if (k >= 1f) v.Duration = 0f;
                }
                else v.Level = v.To;
                if (v.FadingOut && v.Level <= 0.0001f)
                {
                    Silence(v);
                    _voices.RemoveAt(i);
                    continue;
                }
                Apply(v);
            }
        }

        private static void FadeVoice(Voice v, float target, float duration)
        {
            v.From = v.Level;
            v.To = MathHelper.Clamp(target, 0f, 1f);
            v.Time = 0f;
            v.Duration = Math.Max(0f, duration);
            if (v.Duration <= 0f) v.Level = v.To;
        }

        private static SoundEffectInstance? Start(string path)
        {
            if (!Enabled) return null;
            try
            {
                var sound = SoundCache.Get(path);
                if (sound == null) return null;
                var instance = sound.CreateInstance();
                instance.IsLooped = true;
                instance.Volume = 0f;
                instance.Play();
                return instance;
            }
            catch (Exception)
            {
                return null; // sem dispositivo de áudio (máquina sem som, testes, editor)
            }
        }

        private static void Apply(Voice v)
        {
            if (v.Instance == null) return;
            try { v.Instance.Volume = EffectiveVolume(v.Level, v.Volume); }
            catch (Exception) { /* dispositivo indisponível */ }
        }

        private static void Silence(Voice v)
        {
            try { v.Instance?.Stop(); v.Instance?.Dispose(); }
            catch (Exception) { /* ignora */ }
            v.Instance = null;
        }
    }
}
