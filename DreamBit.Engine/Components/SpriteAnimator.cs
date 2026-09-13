using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>Evento disparado ao a animação entrar em um frame específico.</summary>
    public readonly record struct AnimationFrameEvent(int Frame, string Name);

    /// <summary>
    /// Anima frames de uma sprite sheet (grade de frames de tamanho fixo). No play mode
    /// avança os frames pela taxa (FPS), com repetição opcional. (Marco 2 — animação.)
    /// Pode disparar eventos nomeados ao entrar em frames específicos (<see cref="AnimationEvent"/>).
    /// </summary>
    public sealed class SpriteAnimator : SceneComponent
    {
        private string? _texturePath;
        private int _frameWidth = 32;
        private int _frameHeight = 32;
        private int _frameCount = 1;
        private float _fps = 8f;
        private bool _loop = true;
        private Vector2 _size = new(64, 64);

        private int _currentFrame;
        private double _accumulator;
        private readonly Dictionary<int, string> _frameEvents = new();

        public override string DisplayName => "Sprite Animator";

        /// <summary>Disparado quando a animação entra em um frame que tem um evento associado.</summary>
        public event Action<string>? AnimationEvent;

        /// <summary>Eventos por frame (frame → nome). Editar via <see cref="SetEvents"/>.</summary>
        public IEnumerable<AnimationFrameEvent> Events =>
            _frameEvents.Select(kv => new AnimationFrameEvent(kv.Key, kv.Value));

        /// <summary>Define o conjunto de eventos por frame (substitui o atual).</summary>
        public void SetEvents(IEnumerable<AnimationFrameEvent> events)
        {
            _frameEvents.Clear();
            foreach (var e in events)
                if (e.Frame >= 0 && !string.IsNullOrWhiteSpace(e.Name))
                    _frameEvents[e.Frame] = e.Name.Trim();
            OnPropertyChanged(nameof(Events));
        }

        public string? TexturePath { get => _texturePath; set => Set(ref _texturePath, value); }
        public int FrameWidth { get => _frameWidth; set => Set(ref _frameWidth, Math.Max(1, value)); }
        public int FrameHeight { get => _frameHeight; set => Set(ref _frameHeight, Math.Max(1, value)); }
        public int FrameCount { get => _frameCount; set => Set(ref _frameCount, Math.Max(1, value)); }
        public float Fps { get => _fps; set => Set(ref _fps, Math.Max(0f, value)); }
        public bool Loop { get => _loop; set => Set(ref _loop, value); }
        public Vector2 Size { get => _size; set => Set(ref _size, value); }

        public int CurrentFrame => _currentFrame;

        public void Reset()
        {
            _currentFrame = 0;
            _accumulator = 0;
        }

        /// <summary>Avança a animação por um intervalo de tempo (lógica pura, testável).</summary>
        public void Advance(double deltaSeconds)
        {
            if (_fps <= 0f || _frameCount <= 1)
                return;

            _accumulator += deltaSeconds;
            double frameTime = 1.0 / _fps;

            while (_accumulator >= frameTime)
            {
                _accumulator -= frameTime;
                _currentFrame++;

                if (_currentFrame >= _frameCount)
                {
                    if (_loop)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        // Segura no último frame (já disparado ao entrar nele); não re-dispara.
                        _currentFrame = _frameCount - 1;
                        _accumulator = 0;
                        break;
                    }
                }

                FireFrameEvent(_currentFrame); // dispara ao entrar no frame
            }
        }

        private void FireFrameEvent(int frame)
        {
            if (_frameEvents.TryGetValue(frame, out var name))
                AnimationEvent?.Invoke(name);
        }

        protected internal override void Update(GameTime gameTime)
            => Advance(gameTime.ElapsedGameTime.TotalSeconds);

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var texture = TextureCache.Get(drawing.SpriteBatch.GraphicsDevice, _texturePath);
            if (texture == null)
            {
                drawing.DrawQuad(Owner.Transform.WorldMatrix, _size, new Color(110, 110, 120));
                return;
            }

            int columns = Math.Max(1, texture.Width / _frameWidth);
            int frame = Math.Clamp(_currentFrame, 0, _frameCount - 1);
            int col = frame % columns;
            int row = frame / columns;

            var source = new Rectangle(col * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight);
            drawing.DrawFrame(Owner.Transform.WorldMatrix, _size, Color.White, texture, source);
        }
    }
}
