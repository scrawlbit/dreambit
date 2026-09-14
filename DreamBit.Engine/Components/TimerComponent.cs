using DreamBit.Engine.Elements;
using DreamBit.Engine.Messaging;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Conta um tempo e dispara uma mensagem no barramento ao esgotar — para atrasos, spawns
    /// periódicos, timeouts. Pode repetir. Começa no play (se
    /// <see cref="AutoStart"/>) ou ao receber a mensagem <see cref="StartOn"/>.
    /// </summary>
    public sealed class TimerComponent : SceneComponent, IMessageReceiver
    {
        private float _duration = 1f;
        private bool _repeat;
        private bool _autoStart = true;
        private string _sendOnElapsed = "timeout";
        private string _startOn = string.Empty;

        private float _remaining;
        private bool _running;

        public override string DisplayName => "Timer";

        public float Duration { get => _duration; set => Set(ref _duration, value < 0f ? 0f : value); }
        public bool Repeat { get => _repeat; set => Set(ref _repeat, value); }
        public bool AutoStart { get => _autoStart; set => Set(ref _autoStart, value); }
        public string SendOnElapsed { get => _sendOnElapsed; set => Set(ref _sendOnElapsed, value ?? string.Empty); }

        /// <summary>Se preenchido, o timer (re)inicia ao receber esta mensagem.</summary>
        public string StartOn { get => _startOn; set => Set(ref _startOn, value ?? string.Empty); }

        public bool IsRunning => _running;

        public void Restart()
        {
            _remaining = _duration;
            _running = true;
        }

        public void Stop() => _running = false;

        protected internal override void OnPlayStarted()
        {
            _running = false;
            if (_autoStart)
                Restart();
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (!_running)
                return;

            _remaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_remaining > 0f)
                return;

            if (!string.IsNullOrEmpty(_sendOnElapsed))
                Owner?.Scene?.Send(_sendOnElapsed, Owner);

            if (_repeat)
                _remaining += _duration; // mantém overflow
            else
                _running = false;
        }

        public void OnMessage(GameMessage message)
        {
            if (!string.IsNullOrEmpty(_startOn) &&
                string.Equals(message.Name, _startOn, System.StringComparison.OrdinalIgnoreCase))
                Restart();
        }
    }
}
