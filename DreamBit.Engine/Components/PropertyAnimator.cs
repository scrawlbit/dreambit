using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>Propriedade animável por uma trilha do <see cref="PropertyAnimator"/>.</summary>
    public enum AnimChannel
    {
        PositionX, PositionY, Rotation, ScaleX, ScaleY,
        SpriteAlpha, SpriteR, SpriteG, SpriteB
    }

    /// <summary>Um ponto-chave (tempo em segundos, valor).</summary>
    public readonly record struct AnimKey(float Time, float Value);

    /// <summary>Uma trilha: uma propriedade animada por vários keyframes, com easing entre eles.</summary>
    public sealed class PropertyTrack
    {
        public AnimChannel Channel { get; set; }
        public Scrawlbit.EasingMode Easing { get; set; } = Scrawlbit.EasingMode.InOut;
        public List<AnimKey> Keys { get; } = new();

        /// <summary>Valor amostrado no tempo t (segundos), interpolado entre os keyframes.</summary>
        public float Sample(float t)
        {
            if (Keys.Count == 0) return 0f;
            if (Keys.Count == 1 || t <= Keys[0].Time) return Keys[0].Value;
            if (t >= Keys[^1].Time) return Keys[^1].Value;

            for (int i = 0; i < Keys.Count - 1; i++)
            {
                var a = Keys[i];
                var b = Keys[i + 1];
                if (t >= a.Time && t <= b.Time)
                {
                    float span = b.Time - a.Time;
                    float local = span <= 0f ? 0f : (t - a.Time) / span;
                    return Scrawlbit.Mathf.Lerp(a.Value, b.Value, Scrawlbit.Easing.Apply(Easing, local));
                }
            }
            return Keys[^1].Value;
        }

        public float Length => Keys.Count == 0 ? 0f : Keys[^1].Time;
    }

    /// <summary>
    /// Timeline de propriedades: anima qualquer propriedade (posição, rotação, escala, cor do
    /// sprite) por keyframes ao longo do tempo — o equivalente ao AnimationPlayer do Godot /
    /// Animation do Unity, além do Tween (que é de-para simples). Várias trilhas rodam juntas.
    /// </summary>
    public sealed class PropertyAnimator : SceneComponent
    {
        private float _duration = 1f;
        private TweenLoop _loop = TweenLoop.Loop;
        private bool _playOnStart = true;
        private float _time;
        private bool _playing;

        public override string DisplayName => "Property Animator";

        public List<PropertyTrack> Tracks { get; } = new();

        /// <summary>Duração do ciclo (s). Se 0, usa o maior tempo de keyframe das trilhas.</summary>
        public float Duration { get => _duration; set => Set(ref _duration, value < 0f ? 0f : value); }
        public TweenLoop Loop { get => _loop; set => Set(ref _loop, value); }
        public bool PlayOnStart { get => _playOnStart; set => Set(ref _playOnStart, value); }

        public float EffectiveDuration
        {
            get
            {
                if (_duration > 0f) return _duration;
                float max = 0f;
                foreach (var track in Tracks) max = System.Math.Max(max, track.Length);
                return max <= 0f ? 1f : max;
            }
        }

        protected internal override void OnPlayStarted()
        {
            _time = 0f;
            _playing = _playOnStart;
            if (_playing) Apply(0f); // pose inicial
        }

        public void Play() { _time = 0f; _playing = true; }
        public void Stop() => _playing = false;

        protected internal override void Update(GameTime gameTime)
        {
            if (!_playing)
                return;

            _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float dur = EffectiveDuration;

            float sampleT = _loop switch
            {
                TweenLoop.Loop => Scrawlbit.Mathf.Repeat(_time, dur),
                TweenLoop.PingPong => Scrawlbit.Mathf.PingPong(_time, dur),
                _ => System.Math.Min(_time, dur)
            };
            if (_loop == TweenLoop.Once && _time >= dur)
                _playing = false;

            Apply(sampleT);
        }

        private void Apply(float t)
        {
            foreach (var track in Tracks)
                ApplyChannel(track.Channel, track.Sample(t));
        }

        private void ApplyChannel(AnimChannel channel, float value)
        {
            var tr = Owner.Transform;
            switch (channel)
            {
                case AnimChannel.PositionX: tr.Position = new Vector2(value, tr.Position.Y); break;
                case AnimChannel.PositionY: tr.Position = new Vector2(tr.Position.X, value); break;
                case AnimChannel.Rotation: tr.Rotation = MathHelper.ToRadians(value); break;
                case AnimChannel.ScaleX: tr.Scale = new Vector2(value, tr.Scale.Y); break;
                case AnimChannel.ScaleY: tr.Scale = new Vector2(tr.Scale.X, value); break;
                case AnimChannel.SpriteAlpha: SetSpriteChannel(3, value); break;
                case AnimChannel.SpriteR: SetSpriteChannel(0, value); break;
                case AnimChannel.SpriteG: SetSpriteChannel(1, value); break;
                case AnimChannel.SpriteB: SetSpriteChannel(2, value); break;
            }
        }

        private void SetSpriteChannel(int index, float value)
        {
            var sprite = Owner.Components.OfType<SpriteRenderer>().FirstOrDefault();
            if (sprite == null)
                return;
            var c = sprite.Color;
            byte v = (byte)Scrawlbit.Mathf.Clamp(value, 0f, 255f);
            sprite.Color = index switch
            {
                0 => new Color(v, c.G, c.B, c.A),
                1 => new Color(c.R, v, c.B, c.A),
                2 => new Color(c.R, c.G, v, c.A),
                _ => new Color(c.R, c.G, c.B, v)
            };
        }
    }
}
