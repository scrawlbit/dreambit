using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Faz o objeto rolar em ritmo diferente da câmera, criando profundidade (parallax de
    /// fundo). Fator por eixo em [0,1]: 1 = anda junto com o mundo (a câmera passa por cima),
    /// 0 = acompanha a câmera (parece infinitamente distante/fixo). Valores intermediários dão
    /// o efeito de camadas de fundo. Equivale ao ParallaxBackground do Godot.
    /// A posição de origem (âncora) é capturada no início do play.
    /// </summary>
    public sealed class ParallaxLayer : SceneComponent
    {
        private float _factorX = 0.5f;
        private float _factorY = 1f;
        private Vector2 _base;
        private bool _captured;

        public override string DisplayName => "Parallax";

        public float FactorX { get => _factorX; set => Set(ref _factorX, MathHelper.Clamp(value, 0f, 1f)); }
        public float FactorY { get => _factorY; set => Set(ref _factorY, MathHelper.Clamp(value, 0f, 1f)); }

        protected internal override void OnPlayStarted()
        {
            _base = Owner.Transform.Position;
            _captured = true;
            Apply();
        }

        protected internal override void Update(GameTime gameTime) => Apply();

        private void Apply()
        {
            if (Owner == null)
                return;
            if (!_captured) // fora do play (editor parado): usa a posição atual como âncora
                _base = Owner.Transform.Position;

            var cam = Screen.CameraPosition;
            Owner.Transform.Position = new Vector2(
                _base.X + cam.X * (1f - _factorX),
                _base.Y + cam.Y * (1f - _factorY));
        }
    }
}
