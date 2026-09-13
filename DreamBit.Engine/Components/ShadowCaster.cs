using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Oclusor de luz: uma caixa que projeta sombra a partir das <see cref="Light2D"/> (o renderer
    /// desenha o volume de sombra no lightmap, então a luz não atravessa a parede). Um por parede/
    /// obstáculo. Convexo (retângulo) — silhueta calculada por ângulo em relação à luz.
    /// </summary>
    public sealed class ShadowCaster : SceneComponent
    {
        private float _width = 64f;
        private float _height = 64f;

        public override string DisplayName => "Shadow Caster";

        public float Width { get => _width; set => Set(ref _width, System.Math.Max(1f, value)); }
        public float Height { get => _height; set => Set(ref _height, System.Math.Max(1f, value)); }
        public Vector2 Offset { get; set; }

        /// <summary>Os 4 cantos do oclusor em coordenadas de mundo.</summary>
        public void WorldCorners(Vector2[] into)
        {
            var c = Owner.Transform.WorldPosition + Offset;
            float hw = _width / 2f, hh = _height / 2f;
            into[0] = new Vector2(c.X - hw, c.Y - hh);
            into[1] = new Vector2(c.X + hw, c.Y - hh);
            into[2] = new Vector2(c.X + hw, c.Y + hh);
            into[3] = new Vector2(c.X - hw, c.Y + hh);
        }
    }
}
