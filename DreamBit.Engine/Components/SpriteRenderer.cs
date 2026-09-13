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
        private bool _chromaKey;
        private bool _chromaAuto = true;
        private Color _chromaColor = new(255, 0, 255);
        private int _chromaTolerance = 30;

        public override string DisplayName => "Sprite Renderer";

        /// <summary>Remove a cor de fundo da textura (chroma key / green screen).</summary>
        public bool ChromaKeyEnabled { get => _chromaKey; set => Set(ref _chromaKey, value); }
        /// <summary>Detecta a cor de fundo automaticamente (senão usa <see cref="ChromaColor"/>).</summary>
        public bool ChromaAuto { get => _chromaAuto; set => Set(ref _chromaAuto, value); }
        /// <summary>Cor de fundo a remover, quando não é automático.</summary>
        public Color ChromaColor { get => _chromaColor; set => Set(ref _chromaColor, value); }
        /// <summary>Tolerância de cor (0 = exata; maior = mais tons removidos).</summary>
        public int ChromaTolerance { get => _chromaTolerance; set => Set(ref _chromaTolerance, value < 0 ? 0 : value); }

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
            var device = drawing.SpriteBatch.GraphicsDevice;
            var texture = _chromaKey
                ? TextureCache.GetChromaKeyed(device, _texturePath, _chromaAuto, _chromaColor, _chromaTolerance)
                : TextureCache.Get(device, _texturePath);

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
