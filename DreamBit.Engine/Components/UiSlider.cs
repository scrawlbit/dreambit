using DreamBit.Engine.Audio;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Controle deslizante (slider) de UI: arrasta um valor em [0,1] com o ponteiro. Pensado
    /// para menus de opções — pode ligar direto a um bus do <see cref="AudioMixer"/>
    /// (<see cref="BusTarget"/>), virando um controle de volume, e/ou disparar uma mensagem ao
    /// mudar. Espaço de tela (HUD). Equivale ao HSlider do Godot / Slider do Unity.
    /// </summary>
    public sealed class UiSlider : SceneComponent, IUiFocusable
    {
        private float _value = 1f;
        private float _width = 200f;
        private float _height = 20f;
        private Color _track = new(40, 46, 58);
        private Color _fill = new(90, 170, 255);
        private Color _knob = new(220, 228, 240);
        private string _busTarget = string.Empty;
        private string _sendOnChange = string.Empty;

        private bool _dragging;

        public override string DisplayName => "UI Slider";

        /// <summary>Valor atual em [0,1].</summary>
        public float Value { get => _value; set => Set(ref _value, MathHelper.Clamp(value, 0f, 1f)); }
        public float Width { get => _width; set => Set(ref _width, value < 1 ? 1 : value); }
        public float Height { get => _height; set => Set(ref _height, value < 1 ? 1 : value); }
        public Color Track { get => _track; set => Set(ref _track, value); }
        public Color Fill { get => _fill; set => Set(ref _fill, value); }
        public Color Knob { get => _knob; set => Set(ref _knob, value); }

        /// <summary>Se preenchido, o valor controla o volume desse bus do mixer ao vivo.</summary>
        public string BusTarget { get => _busTarget; set => Set(ref _busTarget, value ?? string.Empty); }

        /// <summary>Mensagem disparada quando o valor muda (opcional).</summary>
        public string SendOnChange { get => _sendOnChange; set => Set(ref _sendOnChange, value ?? string.Empty); }

        protected internal override void OnPlayStarted()
        {
            _dragging = false;
            // Inicia o slider refletindo o volume atual do bus, se ligado.
            if (!string.IsNullOrEmpty(_busTarget))
                _value = AudioMixer.GetVolume(_busTarget);
            ApplyBus();
        }

        protected internal override void Update(GameTime gameTime)
        {
            var p = GameInput.PointerPosition;
            var rect = ScreenRect();

            if (GameInput.PointerPressed && rect.Contains((int)p.X, (int)p.Y))
                _dragging = true;
            if (!GameInput.PointerDown)
                _dragging = false;

            if (_dragging)
            {
                float old = _value;
                _value = MathHelper.Clamp((p.X - rect.Left) / rect.Width, 0f, 1f);
                if (_value != old)
                {
                    ApplyBus();
                    if (!string.IsNullOrEmpty(_sendOnChange))
                        Owner?.Scene?.Send(_sendOnChange, Owner);
                }
            }
        }

        private void ApplyBus()
        {
            if (!string.IsNullOrEmpty(_busTarget))
                AudioMixer.SetVolume(_busTarget, _value);
        }

        /// <summary>Retângulo do slider em coordenadas de tela (centrado na posição do objeto).</summary>
        public Rectangle ScreenRect()
        {
            var pos = Owner.Transform.WorldPosition;
            return new Rectangle(
                (int)(pos.X - _width / 2f), (int)(pos.Y - _height / 2f),
                (int)_width, (int)_height);
        }

        public Rectangle FillRect()
        {
            var r = ScreenRect();
            return new Rectangle(r.X, r.Y, (int)(r.Width * _value), r.Height);
        }

        public Rectangle KnobRect()
        {
            var r = ScreenRect();
            int kx = r.X + (int)(r.Width * _value);
            int kw = (int)(_height * 0.8f);
            return new Rectangle(kx - kw / 2, r.Y - 2, kw, r.Height + 4);
        }

        // IUiFocusable
        public bool Focusable => Owner?.IsVisible ?? false;
        public Rectangle FocusRect => ScreenRect();
        public void Activate() { }
        public void Nudge(int dir)
        {
            float old = _value;
            Value = _value + dir * 0.05f;
            if (_value != old)
            {
                ApplyBus();
                if (!string.IsNullOrEmpty(_sendOnChange))
                    Owner?.Scene?.Send(_sendOnChange, Owner);
            }
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            drawing.SpriteBatch.Draw(drawing.Pixel, ScreenRect(), _track);
            drawing.SpriteBatch.Draw(drawing.Pixel, FillRect(), _fill);
            drawing.SpriteBatch.Draw(drawing.Pixel, KnobRect(), _knob);
            UiFocusDraw.Outline(drawing, ScreenRect(), UiFocus.Has(this));
        }
    }
}
