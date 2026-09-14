using DreamBit.Engine.Audio;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Toca um efeito sonoro (WAV) no play mode. Por padrão toca ao iniciar; pode
    /// repetir em loop. Confiável ao rodar o jogo (DreamBit.Player).
    /// </summary>
    public sealed class AudioSource : SceneComponent
    {
        private string? _soundPath;
        private float _volume = 1f;
        private bool _playOnStart = true;
        private bool _loop;
        private string _bus = AudioMixer.Sfx;
        private bool _spatial;
        private float _maxDistance = 600f;

        // Fade da própria fonte (crossfade/camadas de música).
        private float _fadeFrom, _fadeTo, _fadeTime, _fadeElapsed;
        private bool _stopAtEnd;

        private SoundEffectInstance? _instance;

        public override string DisplayName => "Audio Source";

        public string? SoundPath { get => _soundPath; set => Set(ref _soundPath, value); }
        public float Volume { get => _volume; set => Set(ref _volume, MathHelper.Clamp(value, 0f, 1f)); }
        public bool PlayOnStart { get => _playOnStart; set => Set(ref _playOnStart, value); }
        public bool Loop { get => _loop; set => Set(ref _loop, value); }

        /// <summary>Barramento do mixer por onde este som passa (ex.: "Music", "SFX").</summary>
        public string Bus { get => _bus; set => Set(ref _bus, string.IsNullOrEmpty(value) ? AudioMixer.Sfx : value); }

        /// <summary>Se o som é posicional (atenua/pan pela distância ao <see cref="AudioListener"/>).</summary>
        public bool Spatial { get => _spatial; set => Set(ref _spatial, value); }

        /// <summary>Distância (px) em que o som espacial some por completo.</summary>
        public float MaxDistance { get => _maxDistance; set => Set(ref _maxDistance, System.Math.Max(1f, value)); }

        /// <summary>Fatores espaciais (atenuação, pan) deste frame, ou (1,0) se não for espacial.</summary>
        private (float Attenuation, float Pan) Spatialize()
        {
            if (!_spatial || AudioListener.Position is not Vector2 listener || Owner == null)
                return (1f, 0f);
            return AudioSpatial.Compute(Owner.Transform.WorldPosition, listener, _maxDistance);
        }

        private float EffectiveVolume => MathHelper.Clamp(_volume, 0f, 1f) * AudioMixer.Effective(_bus) * Spatialize().Attenuation;

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
            AdvanceFade((float)gameTime.ElapsedGameTime.TotalSeconds);

            // Reflete ao vivo mudanças de volume do bus/Master (ex.: menu de opções) e a
            // posição espacial (atenuação + pan pela distância ao ouvinte).
            if (_instance != null && _instance.State == SoundState.Playing)
            {
                try
                {
                    _instance.Volume = EffectiveVolume;
                    if (_spatial)
                        _instance.Pan = Spatialize().Pan;
                }
                catch { /* motor de áudio indisponível */ }
            }
        }

        public void Stop()
        {
            try { _instance?.Stop(); }
            catch { /* ignora */ }
        }

        /// <summary>Transiciona o volume desta fonte para <paramref name="target"/> em
        /// <paramref name="duration"/> s (para crossfade/camadas de música). Com
        /// <paramref name="stopAtEnd"/>, para o som ao terminar o fade (fade-out).</summary>
        public void FadeTo(float target, float duration, bool stopAtEnd = false)
        {
            _fadeFrom = _volume;
            _fadeTo = MathHelper.Clamp(target, 0f, 1f);
            _fadeTime = System.Math.Max(0.001f, duration);
            _fadeElapsed = 0f;
            _stopAtEnd = stopAtEnd;
        }

        private void AdvanceFade(float dt)
        {
            if (_fadeTime <= 0f)
                return;
            _fadeElapsed += dt;
            float k = MathHelper.Clamp(_fadeElapsed / _fadeTime, 0f, 1f);
            Volume = MathHelper.Lerp(_fadeFrom, _fadeTo, k);
            if (k >= 1f)
            {
                _fadeTime = 0f;
                if (_stopAtEnd)
                    Stop();
            }
        }
    }
}
