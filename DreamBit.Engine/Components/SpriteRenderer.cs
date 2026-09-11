using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Componente que desenha um retângulo colorido (ou textura) no transform do objeto.
    /// Equivalente ao ImageRenderer de DreamBit.Game, na forma mínima para o editor.
    /// </summary>
    public sealed class SpriteRenderer : SceneComponent
    {
        private Vector2 _size = new(120, 80);
        private Color _color = new(70, 130, 200);

        public override string DisplayName => "Sprite Renderer";

        public Vector2 Size
        {
            get => _size;
            set => Set(ref _size, value);
        }

        public Color Color
        {
            get => _color;
            set => Set(ref _color, value);
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            drawing.DrawQuad(Owner.Transform.WorldMatrix, _size, _color);
        }
    }
}
