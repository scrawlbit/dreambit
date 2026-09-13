using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Luz pontual 2D: um disco de luz radial na posição do objeto. O renderer acumula todas as
    /// luzes num lightmap e multiplica sobre a cena (funciona para sprites e tilemap). Combine com
    /// <see cref="AmbientLight"/> para a escuridão de fundo. Sem shaders — usa blend aditivo.
    /// </summary>
    public sealed class Light2D : SceneComponent
    {
        private float _radius = 200f;
        private float _intensity = 1f;
        private Color _color = new(255, 240, 210); // branco quente

        public override string DisplayName => "Light 2D";

        /// <summary>Raio do disco de luz (px).</summary>
        public float Radius { get => _radius; set => Set(ref _radius, System.Math.Max(1f, value)); }

        /// <summary>Intensidade (multiplica a cor; &gt;1 satura para luz forte).</summary>
        public float Intensity { get => _intensity; set => Set(ref _intensity, System.Math.Max(0f, value)); }

        /// <summary>Cor da luz.</summary>
        public Color Color { get => _color; set => Set(ref _color, value); }
    }
}
