using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Interruptor de UI (checkbox): liga/desliga ao clicar, com uma marca quando ligado, e
    /// dispara uma mensagem ao mudar. Espaço de tela (HUD). Equivale ao CheckBox de Godot/Unity.
    /// </summary>
    public sealed class UiToggle : SceneComponent
    {
        private bool _isOn;
        private float _size = 28f;
        private Color _box = new(40, 46, 58);
        private Color _check = new(90, 200, 140);
        private string _sendOnChange = string.Empty;

        private bool _armed;

        public override string DisplayName => "UI Toggle";

        public bool IsOn { get => _isOn; set => Set(ref _isOn, value); }
        public float Size { get => _size; set => Set(ref _size, value < 1 ? 1 : value); }
        public Color Box { get => _box; set => Set(ref _box, value); }
        public Color Check { get => _check; set => Set(ref _check, value); }
        public string SendOnChange { get => _sendOnChange; set => Set(ref _sendOnChange, value ?? string.Empty); }

        protected internal override void OnPlayStarted() => _armed = false;

        protected internal override void Update(GameTime gameTime)
        {
            var p = GameInput.PointerPosition;
            bool inside = ScreenRect().Contains((int)p.X, (int)p.Y);

            if (inside && GameInput.PointerPressed)
                _armed = true;

            if (GameInput.PointerReleased)
            {
                if (_armed && inside)
                {
                    IsOn = !IsOn;
                    if (!string.IsNullOrEmpty(_sendOnChange))
                        Owner?.Scene?.Send(_sendOnChange, Owner);
                }
                _armed = false;
            }
            if (!GameInput.PointerDown)
                _armed = false;
        }

        public Rectangle ScreenRect()
        {
            var pos = Owner.Transform.WorldPosition;
            return new Rectangle((int)(pos.X - _size / 2f), (int)(pos.Y - _size / 2f), (int)_size, (int)_size);
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var r = ScreenRect();
            drawing.SpriteBatch.Draw(drawing.Pixel, r, _box);
            if (_isOn)
            {
                int pad = (int)(_size * 0.22f);
                drawing.SpriteBatch.Draw(drawing.Pixel,
                    new Rectangle(r.X + pad, r.Y + pad, r.Width - pad * 2, r.Height - pad * 2), _check);
            }
        }
    }
}
