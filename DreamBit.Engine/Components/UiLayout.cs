using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>Direção de empilhamento de um <see cref="UiLayout"/>.</summary>
    public enum LayoutDirection { Vertical, Horizontal }

    /// <summary>
    /// Empilha os objetos-filhos em coluna ou linha, com espaçamento — para montar menus e
    /// painéis de HUD (uma pilha de botões, por exemplo) sem posicionar cada item na mão.
    /// Reposiciona a cada frame, então acompanha mudanças. Combine com <see cref="UiAnchor"/> no objeto-pai.
    /// </summary>
    public sealed class UiLayout : SceneComponent
    {
        private LayoutDirection _direction = LayoutDirection.Vertical;
        private float _spacing = 12f;

        public override string DisplayName => "UI Layout";

        public LayoutDirection Direction { get => _direction; set => Set(ref _direction, value); }
        public float Spacing { get => _spacing; set => Set(ref _spacing, value); }

        protected internal override void OnPlayStarted() => Arrange();
        protected internal override void Update(GameTime gameTime) => Arrange();

        /// <summary>Posiciona os filhos em sequência a partir da origem do objeto (posições locais).</summary>
        public void Arrange()
        {
            if (Owner == null || Owner.Children.Count == 0)
                return;

            // Extensão total para centralizar a pilha na origem do container.
            float total = 0f;
            foreach (var child in Owner.Children)
                total += Extent(child) + _spacing;
            total -= _spacing;

            float cursor = -total / 2f;
            foreach (var child in Owner.Children)
            {
                float ext = Extent(child);
                float center = cursor + ext / 2f;
                child.Transform.Position = _direction == LayoutDirection.Vertical
                    ? new Vector2(0f, center)
                    : new Vector2(center, 0f);
                cursor += ext + _spacing;
            }
        }

        /// <summary>Tamanho do filho no eixo do layout (botão, sprite ou padrão).</summary>
        private float Extent(GameObject child)
        {
            foreach (var component in child.Components)
            {
                if (component is UiButton button)
                    return _direction == LayoutDirection.Vertical ? button.Height : button.Width;
                if (component is SpriteRenderer sprite)
                    return _direction == LayoutDirection.Vertical ? sprite.Size.Y : sprite.Size.X;
            }
            return 40f;
        }
    }
}
