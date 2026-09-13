using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Desenha texto com a fonte pixel embutida. No mundo (rótulo flutuante) ou em
    /// <see cref="ScreenSpace"/> (HUD: pontuação, vidas), fixo na tela. Base para UI.
    /// </summary>
    public sealed class TextRenderer : SceneComponent
    {
        private string _text = "TEXTO";
        private string _locKey = string.Empty;
        private Color _color = Color.White;
        private int _pixelSize = 4;
        private bool _screenSpace;

        public override string DisplayName => "Text";

        public string Text { get => _text; set => Set(ref _text, value ?? string.Empty); }

        /// <summary>Chave de localização (opcional): se preenchida, o texto exibido vem de
        /// <see cref="Localization.Localizer"/> no idioma atual; senão usa <see cref="Text"/>.</summary>
        public string LocKey { get => _locKey; set => Set(ref _locKey, value ?? string.Empty); }

        /// <summary>Texto efetivamente exibido (localizado quando há <see cref="LocKey"/>).</summary>
        public string DisplayText => string.IsNullOrEmpty(_locKey)
            ? _text
            : Localization.Localizer.Get(_locKey, _text);
        public Color Color { get => _color; set => Set(ref _color, value); }

        /// <summary>Tamanho de cada "pixel" da fonte (escala).</summary>
        public int PixelSize { get => _pixelSize; set => Set(ref _pixelSize, value < 1 ? 1 : value); }

        /// <summary>Se true, desenha em espaço de tela (HUD), fixo; senão no mundo.</summary>
        public bool ScreenSpace { get => _screenSpace; set => Set(ref _screenSpace, value); }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            if (_screenSpace)
                return; // HUD desenha no passe de tela

            // Rótulo no mundo, centrado no objeto.
            var p = Owner.Transform.WorldPosition;
            int w = PixelFont.MeasureWidth(DisplayText) * _pixelSize;
            int h = PixelFont.GlyphHeight * _pixelSize;
            DrawText(drawing, (int)p.X - w / 2, (int)p.Y - h / 2);
        }

        protected internal override void DrawScreen(ISceneDrawing drawing)
        {
            if (!_screenSpace)
                return;

            // HUD: canto superior-esquerdo na posição do objeto (coordenadas de tela).
            var p = Owner.Transform.Position;
            DrawText(drawing, (int)p.X, (int)p.Y);
        }

        private void DrawText(ISceneDrawing drawing, int originX, int originY)
        {
            int px = _pixelSize;
            int cursor = originX;

            foreach (char ch in DisplayText)
            {
                var glyph = PixelFont.Glyph(ch);
                for (int row = 0; row < PixelFont.GlyphHeight; row++)
                {
                    byte bits = glyph[row];
                    for (int col = 0; col < PixelFont.GlyphWidth; col++)
                        if (((bits >> (PixelFont.GlyphWidth - 1 - col)) & 1) != 0)
                            drawing.SpriteBatch.Draw(drawing.Pixel,
                                new Rectangle(cursor + col * px, originY + row * px, px, px), _color);
                }
                cursor += (PixelFont.GlyphWidth + PixelFont.Spacing) * px;
            }
        }
    }
}
