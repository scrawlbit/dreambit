using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>Canal do transform que o Tween anima.</summary>
    public enum TweenChannel { PositionX, PositionY, Rotation, ScaleX, ScaleY }

    /// <summary>Repetição do Tween.</summary>
    public enum TweenLoop { Once, Loop, PingPong }

    /// <summary>
    /// Interpola uma propriedade do transform (posição/rotação/escala) de From a To ao longo
    /// de Duration com easing — o "juice" barato (equivalente ao Tween do Godot / DOTween).
    /// Rotação é em graus. No play, anima o dono.
    /// </summary>
    public sealed class TweenComponent : SceneComponent
    {
        private TweenChannel _channel = TweenChannel.PositionY;
        private float _from;
        private float _to = 100f;
        private float _duration = 1f;
        private Scrawlbit.EasingMode _easing = Scrawlbit.EasingMode.InOut;
        private TweenLoop _loop = TweenLoop.PingPong;
        private bool _playOnStart = true;

        private float _time;
        private bool _playing;

        public override string DisplayName => "Tween";

        public TweenChannel Channel { get => _channel; set => Set(ref _channel, value); }
        public float From { get => _from; set => Set(ref _from, value); }
        public float To { get => _to; set => Set(ref _to, value); }
        public float Duration { get => _duration; set => Set(ref _duration, value < 0.01f ? 0.01f : value); }
        public Scrawlbit.EasingMode Easing { get => _easing; set => Set(ref _easing, value); }
        public TweenLoop Loop { get => _loop; set => Set(ref _loop, value); }
        public bool PlayOnStart { get => _playOnStart; set => Set(ref _playOnStart, value); }

        protected internal override void OnPlayStarted()
        {
            _time = 0f;
            _playing = _playOnStart;
        }

        public void Play() { _time = 0f; _playing = true; }

        protected internal override void Update(GameTime gameTime)
        {
            if (!_playing)
                return;

            _time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            float t;
            switch (_loop)
            {
                case TweenLoop.Loop:
                    t = Scrawlbit.Mathf.Repeat(_time, _duration) / _duration;
                    break;
                case TweenLoop.PingPong:
                    t = Scrawlbit.Mathf.PingPong(_time, _duration) / _duration;
                    break;
                default: // Once
                    t = Scrawlbit.Mathf.Clamp01(_time / _duration);
                    if (_time >= _duration) _playing = false;
                    break;
            }

            float value = Scrawlbit.Mathf.Lerp(_from, _to, Scrawlbit.Easing.Apply(_easing, t));
            Apply(value);
        }

        private void Apply(float value)
        {
            var tr = Owner.Transform;
            switch (_channel)
            {
                case TweenChannel.PositionX: tr.Position = new Vector2(value, tr.Position.Y); break;
                case TweenChannel.PositionY: tr.Position = new Vector2(tr.Position.X, value); break;
                case TweenChannel.Rotation: tr.Rotation = MathHelper.ToRadians(value); break;
                case TweenChannel.ScaleX: tr.Scale = new Vector2(value, tr.Scale.Y); break;
                case TweenChannel.ScaleY: tr.Scale = new Vector2(tr.Scale.X, value); break;
            }
        }
    }
}
