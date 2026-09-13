using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Barra de progresso de UI (só leitura): mostra um valor em [0,1] — barra de vida, energia,
    /// carregamento. Scripts ajustam <see cref="Value"/>. Espaço de tela (HUD) ou no mundo (barra
    /// de vida sobre um inimigo). Equivale ao ProgressBar/TextureProgress de Godot/Unity.
    /// </summary>
    public sealed class UiProgressBar : SceneComponent
    {
        private float _value = 1f;
        private float _width = 120f;
        private float _height = 14f;
        private Color _track = new(40, 46, 58);
        private Color _fill = new(90, 200, 140);

        public override string DisplayName => "UI Progress Bar";

        public float Value { get => _value; set => Set(ref _value, MathHelper.Clamp(value, 0f, 1f)); }
        public float Width { get => _width; set => Set(ref _width, value < 1 ? 1 : value); }
        public float Height { get => _height; set => Set(ref _height, value < 1 ? 1 : value); }
        public Color Track { get => _track; set => Set(ref _track, value); }
        public Color Fill { get => _fill; set => Set(ref _fill, value); }

        public Rectangle ScreenRect()
        {
            var pos = Owner.Transform.WorldPosition;
            return new Rectangle((int)(pos.X - _width / 2f), (int)(pos.Y - _height / 2f), (int)_width, (int)_height);
        }

        public Rectangle FillRect()
        {
            var r = ScreenRect();
            return new Rectangle(r.X, r.Y, (int)(r.Width * _value), r.Height);
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            drawing.SpriteBatch.Draw(drawing.Pixel, ScreenRect(), _track);
            drawing.SpriteBatch.Draw(drawing.Pixel, FillRect(), _fill);
        }
    }
}
