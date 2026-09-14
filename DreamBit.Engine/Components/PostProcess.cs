using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Pós-processamento de tela por shader (efeito compilado no MGCB): satura/dessatura a cena
    /// e aplica um tom (tint). O host (Player) renderiza a cena num render target e a desenha com
    /// o shader. Um por cena. Ex.: dessaturar em pausa/morte, tint sépia/noturno.
    /// </summary>
    public sealed class PostProcess : SceneComponent
    {
        private float _saturation = 1f;
        private Color _tint = Color.White;

        public override string DisplayName => "Post Process";

        /// <summary>Saturação: 1 = cores normais, 0 = tons de cinza.</summary>
        public float Saturation { get => _saturation; set => Set(ref _saturation, MathHelper.Clamp(value, 0f, 1f)); }

        /// <summary>Tom multiplicado sobre a cena (branco = sem efeito).</summary>
        public Color Tint { get => _tint; set => Set(ref _tint, value); }
    }
}
