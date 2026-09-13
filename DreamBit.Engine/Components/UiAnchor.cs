using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>Ponto de ancoragem na tela (canto/borda/centro).</summary>
    public enum AnchorPoint
    {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, Center, MiddleRight,
        BottomLeft, BottomCenter, BottomRight
    }

    /// <summary>
    /// Fixa um objeto HUD em relação às bordas da tela: canto/borda/centro + deslocamento em
    /// pixels. Recalcula a cada frame, então acompanha o redimensionamento da janela — como
    /// as âncoras de UI de Godot (Control anchors) e Unity (RectTransform). Só faz efeito em
    /// objetos em espaço de tela (<see cref="GameObject.ScreenSpace"/>).
    /// </summary>
    public sealed class UiAnchor : SceneComponent
    {
        private AnchorPoint _anchor = AnchorPoint.TopLeft;
        private float _offsetX;
        private float _offsetY;

        public override string DisplayName => "UI Anchor";

        public AnchorPoint Anchor { get => _anchor; set => Set(ref _anchor, value); }
        public float OffsetX { get => _offsetX; set => Set(ref _offsetX, value); }
        public float OffsetY { get => _offsetY; set => Set(ref _offsetY, value); }

        protected internal override void OnPlayStarted() => Apply();
        protected internal override void Update(GameTime gameTime) => Apply();

        /// <summary>Posição de tela resultante para a âncora atual (usada no editor e no runtime).</summary>
        public Vector2 Resolve(int screenWidth, int screenHeight)
        {
            float x = _anchor switch
            {
                AnchorPoint.TopLeft or AnchorPoint.MiddleLeft or AnchorPoint.BottomLeft => 0f,
                AnchorPoint.TopCenter or AnchorPoint.Center or AnchorPoint.BottomCenter => screenWidth / 2f,
                _ => screenWidth
            };
            float y = _anchor switch
            {
                AnchorPoint.TopLeft or AnchorPoint.TopCenter or AnchorPoint.TopRight => 0f,
                AnchorPoint.MiddleLeft or AnchorPoint.Center or AnchorPoint.MiddleRight => screenHeight / 2f,
                _ => screenHeight
            };
            return new Vector2(x + _offsetX, y + _offsetY);
        }

        private void Apply()
        {
            if (Owner == null || !Owner.EffectiveScreenSpace)
                return;
            Owner.Transform.Position = Resolve(Screen.Width, Screen.Height);
        }
    }
}
