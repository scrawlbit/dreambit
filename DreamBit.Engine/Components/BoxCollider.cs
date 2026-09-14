using System;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Colisor sólido em caixa (AABB): marca o objeto como sólido — o
    /// PlatformerController é bloqueado por todos os lados (parede/plataforma sólida),
    /// diferente das ledges (one-way). Alinhado aos eixos (ignora rotação).
    /// </summary>
    public sealed class BoxCollider : SceneComponent
    {
        private Vector2 _size = new(64, 64);
        private Vector2 _offset = Vector2.Zero;

        public override string DisplayName => "Box Collider";

        /// <summary>Tamanho da caixa (largura × altura).</summary>
        public Vector2 Size { get => _size; set => Set(ref _size, value); }

        /// <summary>Deslocamento do centro da caixa em relação ao objeto.</summary>
        public Vector2 Offset { get => _offset; set => Set(ref _offset, value); }

        /// <summary>Caixa AABB em coordenadas de mundo, sem rotação.</summary>
        public Geometry.Aabb Bounds()
            => Geometry.Aabb.FromCenterSize(Owner.Transform.WorldPosition + _offset, _size);

        /// <summary>Caixa em coordenadas de mundo (min, max), sem rotação.</summary>
        public (Vector2 Min, Vector2 Max) WorldBounds()
        {
            var b = Bounds();
            return (b.Min, b.Max);
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            // Contorno leve do colisor (útil no editor).
            var world = Matrix.CreateTranslation(_offset.X, _offset.Y, 0f) * Owner.Transform.WorldMatrix;
            drawing.DrawQuad(world, _size, new Color(90, 200, 255) * 0.18f);
        }
    }
}
