using System.Linq;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Área que recebe golpes: quando uma <see cref="Hitbox"/> de outro time a sobrepõe, o dano
    /// vai para o <see cref="Health"/> do mesmo objeto. Uma caixa AABB em torno do objeto.
    /// </summary>
    public sealed class Hurtbox : SceneComponent
    {
        private float _width = 48f;
        private float _height = 48f;

        public override string DisplayName => "Hurtbox";

        public float Width { get => _width; set => Set(ref _width, System.Math.Max(1f, value)); }
        public float Height { get => _height; set => Set(ref _height, System.Math.Max(1f, value)); }
        public Vector2 Offset { get; set; }

        /// <summary>Time do dono (uma Hitbox só fere times diferentes do seu).</summary>
        public int Team { get; set; }

        /// <summary>O componente de vida do dono (destino do dano), se houver.</summary>
        public Health? Health => Owner?.Components.OfType<Health>().FirstOrDefault();

        /// <summary>Caixa AABB em coordenadas de mundo.</summary>
        public Geometry.Aabb Bounds()
            => Geometry.Aabb.FromCenterSize(Owner.Transform.WorldPosition + Offset, new Vector2(_width, _height));
    }
}
