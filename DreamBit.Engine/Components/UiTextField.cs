using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Campo de texto editável de UI: clicar dá foco, o teclado digita, Enter confirma (dispara
    /// mensagem). Um campo por vez tem o foco (<see cref="UiFocus"/>). Espaço de tela (HUD).
    /// O host precisa alimentar o texto
    /// digitado em <see cref="Input.Input.PushText"/> (o Player faz isso pelo evento da janela).
    /// </summary>
    public sealed class UiTextField : SceneComponent, IUiFocusable
    {
        private string _text = string.Empty;
        private string _placeholder = "…";
        private float _width = 220f;
        private float _height = 34f;
        private int _pixelSize = 3;
        private int _maxLength = 32;
        private Color _box = new(30, 34, 44);
        private Color _boxFocused = new(50, 58, 74);
        private Color _textColor = Color.White;
        private string _sendOnSubmit = string.Empty;

        public override string DisplayName => "UI Text Field";

        public string Text { get => _text; set => Set(ref _text, value ?? string.Empty); }
        public string Placeholder { get => _placeholder; set => Set(ref _placeholder, value ?? string.Empty); }
        public float Width { get => _width; set => Set(ref _width, value < 1 ? 1 : value); }
        public float Height { get => _height; set => Set(ref _height, value < 1 ? 1 : value); }
        public int PixelSize { get => _pixelSize; set => Set(ref _pixelSize, value < 1 ? 1 : value); }
        public int MaxLength { get => _maxLength; set => Set(ref _maxLength, value < 0 ? 0 : value); }
        public Color Box { get => _box; set => Set(ref _box, value); }
        public Color BoxFocused { get => _boxFocused; set => Set(ref _boxFocused, value); }
        public Color TextColor { get => _textColor; set => Set(ref _textColor, value); }
        /// <summary>Mensagem disparada ao confirmar com Enter.</summary>
        public string SendOnSubmit { get => _sendOnSubmit; set => Set(ref _sendOnSubmit, value ?? string.Empty); }

        public bool IsFocused => UiFocus.Has(this);

        // IUiFocusable
        public bool Focusable => Owner?.IsVisible ?? false;
        public Rectangle FocusRect => ScreenRect();
        public void Activate() => UiFocus.Set(this); // ao confirmar, entra em edição
        public void Nudge(int dir) { }

        protected internal override void OnPlayStarted()
        {
            if (IsFocused) UiFocus.Clear();
        }

        protected internal override void Update(GameTime gameTime)
        {
            var p = GameInput.PointerPosition;
            bool inside = ScreenRect().Contains((int)p.X, (int)p.Y);

            if (GameInput.PointerPressed)
            {
                if (inside) UiFocus.Set(this);
                else if (IsFocused) UiFocus.Clear();
            }

            if (!IsFocused)
                return;

            // Backspace por tecla (caso o texto digitado não traga '\b').
            if (GameInput.KeyJustPressed(Keys.Back) && _text.Length > 0)
                Text = _text[..^1];

            foreach (char c in GameInput.TypedText)
            {
                if (c == '\b')
                {
                    if (_text.Length > 0) Text = _text[..^1];
                }
                else if (c == '\r' || c == '\n')
                {
                    if (!string.IsNullOrEmpty(_sendOnSubmit))
                        Owner?.Scene?.Send(_sendOnSubmit, Owner);
                    UiFocus.Clear();
                    break;
                }
                else if (c >= ' ' && (_maxLength <= 0 || _text.Length < _maxLength))
                {
                    Text = _text + c;
                }
            }
        }

        public Rectangle ScreenRect()
        {
            var pos = Owner.Transform.WorldPosition;
            return new Rectangle((int)(pos.X - _width / 2f), (int)(pos.Y - _height / 2f), (int)_width, (int)_height);
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var r = ScreenRect();
            drawing.SpriteBatch.Draw(drawing.Pixel, r, IsFocused ? _boxFocused : _box);

            string shown = _text.Length > 0 ? _text : _placeholder;
            var color = _text.Length > 0 ? _textColor : new Color(_textColor, 0.4f);
            int pad = 6;
            int ty = r.Y + (r.Height - PixelFont.GlyphHeight * _pixelSize) / 2;
            int cursorX = DrawText(drawing, shown, r.X + pad, ty, color);

            // Cursor piscando quando focado.
            if (IsFocused && ((int)(System.Environment.TickCount / 500) % 2 == 0))
                drawing.SpriteBatch.Draw(drawing.Pixel,
                    new Rectangle(cursorX + 1, ty, _pixelSize, PixelFont.GlyphHeight * _pixelSize), _textColor);

            UiFocusDraw.Outline(drawing, r, IsFocused);
        }

        private int DrawText(ISceneDrawing drawing, string text, int originX, int originY, Color color)
        {
            int px = _pixelSize;
            int cursor = originX;
            foreach (char ch in text)
            {
                var glyph = PixelFont.Glyph(ch);
                for (int row = 0; row < PixelFont.GlyphHeight; row++)
                {
                    byte bits = glyph[row];
                    for (int col = 0; col < PixelFont.GlyphWidth; col++)
                        if (((bits >> (PixelFont.GlyphWidth - 1 - col)) & 1) != 0)
                            drawing.SpriteBatch.Draw(drawing.Pixel,
                                new Rectangle(cursor + col * px, originY + row * px, px, px), color);
                }
                cursor += (PixelFont.GlyphWidth + PixelFont.Spacing) * px;
            }
            return cursor;
        }
    }
}
