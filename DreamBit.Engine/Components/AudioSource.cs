using DreamBit.Engine.Audio;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Toca um efeito sonoro (WAV) no play mode. Por padrão toca ao iniciar; pode
    /// repetir em loop. Confiável ao rodar o jogo (DreamBit.Player). (Marco 3.)
    /// </summary>
    public sealed class AudioSource : SceneComponent
    {
        private string? _soundPath;
        private float _volume = 1f;
        private bool _playOnStart = true;
        private bool _loop;
        private string _bus = AudioMixer.Sfx;

        private SoundEffectInstance? _instance;

        public override string DisplayName => "Audio Source";

        public string? SoundPath { get => _soundPath; set => Set(ref _soundPath, value); }
        public float Volume { get => _volume; set => Set(ref _volume, MathHelper.Clamp(value, 0f, 1f)); }
        public bool PlayOnStart { get => _playOnStart; set => Set(ref _playOnStart, value); }
        public bool Loop { get => _loop; set => Set(ref _loop, value); }

        /// <summary>Barramento do mixer por onde este som passa (ex.: "Music", "SFX").</summary>
        public string Bus { get => _bus; set => Set(ref _bus, string.IsNullOrEmpty(value) ? AudioMixer.Sfx : value); }

        private float EffectiveVolume => MathHelper.Clamp(_volume, 0f, 1f) * AudioMixer.Effective(_bus);

        protected internal override void OnPlayStarted()
        {
            if (_playOnStart)
                Play();
        }

        public void Play()
        {
            var sound = SoundCache.Get(_soundPath);
            if (sound == null)
                return;

            try
            {
                _instance?.Stop();
                _instance = sound.CreateInstance();
                _instance.Volume = EffectiveVolume;
                _instance.IsLooped = _loop;
                _instance.Play();
            }
            catch
            {
                // motor de áudio indisponível (ex.: no editor sem Game ativo)
            }
        }

        protected internal override void Update(GameTime gameTime)
        {
            // Reflete ao vivo mudanças de volume do bus/Master (ex.: menu de opções).
            if (_instance != null && _instance.State == SoundState.Playing)
            {
                try { _instance.Volume = EffectiveVolume; }
                catch { /* motor de áudio indisponível */ }
            }
        }

        public void Stop()
        {
            try { _instance?.Stop(); }
            catch { /* ignora */ }
        }
    }
}
