using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Botão de UI clicável. Testa o ponteiro (mouse/toque) contra um retângulo centrado na
    /// posição do objeto (espaço de tela), com estados normal/hover/pressed, e dispara uma
    /// mensagem no barramento ao ser clicado — que um <see cref="MessageListener"/> ou script
    /// pode ouvir. Pensado para HUD/menus; combine com <see cref="UiAnchor"/> e um filho de
    /// texto para o rótulo. Equivale a Button de Godot/Unity.
    /// </summary>
    public sealed class UiButton : SceneComponent
    {
        private float _width = 160f;
        private float _height = 48f;
        private Color _normal = new(60, 70, 90);
        private Color _hover = new(90, 110, 150);
        private Color _pressed = new(40, 50, 70);
        private string _sendOnClick = "click";

        private bool _isHover;
        private bool _isPressed;

        public override string DisplayName => "UI Button";

        public float Width { get => _width; set => Set(ref _width, value < 1 ? 1 : value); }
        public float Height { get => _height; set => Set(ref _height, value < 1 ? 1 : value); }
        public Color Normal { get => _normal; set => Set(ref _normal, value); }
        public Color Hover { get => _hover; set => Set(ref _hover, value); }
        public Color Pressed { get => _pressed; set => Set(ref _pressed, value); }

        /// <summary>Mensagem enviada no clique (release dentro do botão).</summary>
        public string SendOnClick { get => _sendOnClick; set => Set(ref _sendOnClick, value ?? string.Empty); }

        /// <summary>True enquanto o ponteiro está sobre o botão (para o editor/observadores).</summary>
        public bool IsHover => _isHover;

        protected internal override void OnPlayStarted()
        {
            _isHover = false;
            _isPressed = false;
        }

        protected internal override void Update(GameTime gameTime)
        {
            var p = GameInput.PointerPosition;
            _isHover = Contains(p);

            if (_isHover && GameInput.PointerPressed)
                _isPressed = true;

            if (GameInput.PointerReleased)
            {
                if (_isPressed && _isHover && !string.IsNullOrEmpty(_sendOnClick))
                    Owner?.Scene?.Send(_sendOnClick, Owner);
                _isPressed = false;
            }

            if (!GameInput.PointerDown)
                _isPressed = false;
        }

        /// <summary>Retângulo do botão em coordenadas de tela (centrado na posição do objeto).
        /// Usa a posição de mundo — que, num objeto HUD, equivale à coordenada de tela — para
        /// funcionar também quando o botão é filho de um painel (UiLayout/UiAnchor).</summary>
        public Rectangle ScreenRect()
        {
            var pos = Owner.Transform.WorldPosition;
            return new Rectangle(
                (int)(pos.X - _width / 2f), (int)(pos.Y - _height / 2f),
                (int)_width, (int)_height);
        }

        public bool Contains(Vector2 screenPoint) => ScreenRect().Contains((int)screenPoint.X, (int)screenPoint.Y);

        public Color CurrentColor => _isPressed ? _pressed : _isHover ? _hover : _normal;

        protected internal override void Draw(ISceneDrawing drawing)
        {
            // Owner em espaço de tela => este Draw roda no passe de HUD (sem câmera).
            drawing.SpriteBatch.Draw(drawing.Pixel, ScreenRect(), CurrentColor);
        }
    }
}
