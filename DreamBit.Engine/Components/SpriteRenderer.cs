using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Componente que desenha uma textura (PNG) ou, na ausência dela, um retângulo
    /// colorido no transform do objeto. Equivalente ao ImageRenderer de DreamBit.Game.
    /// </summary>
    public sealed class SpriteRenderer : SceneComponent
    {
        private Vector2 _size = new(120, 80);
        private Color _color = new(70, 130, 200);
        private string? _texturePath;

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

        /// <summary>Caminho do arquivo de imagem (PNG). Vazio = retângulo colorido.</summary>
        public string? TexturePath
        {
            get => _texturePath;
            set => Set(ref _texturePath, value);
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var texture = TextureCache.Get(drawing.SpriteBatch.GraphicsDevice, _texturePath);
            var tint = texture != null ? Color.White : _color;
            drawing.DrawQuad(Owner.Transform.WorldMatrix, _size, tint, texture);
        }
    }
}
