using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Contêiner com rolagem: empilha os objetos-filhos numa coluna e mostra só a parte que
    /// cabe na viewport, rolando com a roda do mouse. Itens totalmente fora são ocultados
    /// (culling por item — ideal para listas/menus). Espaço de tela (HUD). Combine com
    /// <see cref="UiAnchor"/> no objeto-pai.
    /// </summary>
    public sealed class UiScrollView : SceneComponent
    {
        private float _width = 240f;
        private float _height = 260f;
        private float _spacing = 8f;
        private float _scroll;
        private float _scrollSpeed = 24f;
        private Color _background = new(22, 25, 32);

        public override string DisplayName => "UI Scroll View";

        public float Width { get => _width; set => Set(ref _width, value < 1 ? 1 : value); }
        public float Height { get => _height; set => Set(ref _height, value < 1 ? 1 : value); }
        public float Spacing { get => _spacing; set => Set(ref _spacing, value); }
        public float ScrollSpeed { get => _scrollSpeed; set => Set(ref _scrollSpeed, value); }
        public Color Background { get => _background; set => Set(ref _background, value); }

        /// <summary>Deslocamento vertical atual (px), 0 = topo.</summary>
        public float Scroll { get => _scroll; set => Set(ref _scroll, value); }

        protected internal override void OnPlayStarted() => Arrange();

        protected internal override void Update(GameTime gameTime)
        {
            if (Owner == null || Owner.Children.Count == 0)
                return;

            var rect = ScreenRect();
            var p = GameInput.PointerPosition;
            if (rect.Contains((int)p.X, (int)p.Y) && GameInput.WheelDelta != 0)
                _scroll -= GameInput.WheelDelta * _scrollSpeed;

            Arrange();
        }

        public Rectangle ScreenRect()
        {
            var pos = Owner.Transform.WorldPosition;
            return new Rectangle((int)(pos.X - _width / 2f), (int)(pos.Y - _height / 2f), (int)_width, (int)_height);
        }

        private void Arrange()
        {
            float content = 0f;
            foreach (var child in Owner.Children)
                content += Extent(child) + _spacing;
            content -= _spacing;

            float maxScroll = System.Math.Max(0f, content - _height);
            _scroll = MathHelper.Clamp(_scroll, 0f, maxScroll);

            float ownerY = Owner.Transform.WorldPosition.Y;
            float vpTop = ownerY - _height / 2f;
            float vpBottom = ownerY + _height / 2f;

            float cursor = -_height / 2f - _scroll; // topo do conteúdo, deslocado pela rolagem
            foreach (var child in Owner.Children)
            {
                float ext = Extent(child);
                float localY = cursor + ext / 2f;
                child.Transform.Position = new Vector2(0f, localY);

                float worldY = ownerY + localY;
                bool inside = worldY - ext / 2f >= vpTop - 0.5f && worldY + ext / 2f <= vpBottom + 0.5f;
                child.IsVisible = inside; // culling por item

                cursor += ext + _spacing;
            }
        }

        private static float Extent(GameObject child)
        {
            foreach (var component in child.Components)
            {
                if (component is UiButton b) return b.Height;
                if (component is UiToggle t) return t.Size;
                if (component is UiTextField f) return f.Height;
                if (component is SpriteRenderer s) return s.Size.Y;
            }
            return 40f;
        }

        protected internal override void Draw(ISceneDrawing drawing)
            => drawing.SpriteBatch.Draw(drawing.Pixel, ScreenRect(), _background);
    }
}
