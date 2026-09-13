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
        private Rectangle? _sourceRect;

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

        /// <summary>
        /// Recorte (região de origem) da textura, para usar um sprite de um atlas/spritesheet.
        /// Null = usa a textura inteira. Largura/altura ≤ 0 são ignoradas (tratadas como null).
        /// </summary>
        public Rectangle? SourceRect
        {
            get => _sourceRect;
            set => Set(ref _sourceRect, value is { Width: > 0, Height: > 0 } ? value : null);
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var texture = TextureCache.Get(drawing.SpriteBatch.GraphicsDevice, _texturePath);

            if (texture != null && _sourceRect is { } src)
            {
                drawing.DrawFrame(Owner.Transform.WorldMatrix, _size, Color.White, texture, src);
                return;
            }

            var tint = texture != null ? Color.White : _color;
            drawing.DrawQuad(Owner.Transform.WorldMatrix, _size, tint, texture);
        }
    }
}
