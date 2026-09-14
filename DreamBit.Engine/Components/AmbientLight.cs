using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Luz ambiente da cena: a cor de base do lightmap (a "escuridão" onde não há luzes). Um por
    /// cena. Sem ele, o renderer usa um ambiente escuro padrão quando há alguma <see cref="Light2D"/>.
    /// </summary>
    public sealed class AmbientLight : SceneComponent
    {
        private Color _color = new(40, 44, 60); // noite azulada

        public override string DisplayName => "Ambient Light";

        /// <summary>Cor ambiente (multiplica a cena onde não há luz). Preto = escuro total.</summary>
        public Color Color { get => _color; set => Set(ref _color, value); }
    }
}
