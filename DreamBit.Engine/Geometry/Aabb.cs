using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Geometry
{
    /// <summary>
    /// Retângulo alinhado aos eixos (AABB) em coordenadas de mundo, com testes de sobreposição
    /// e contenção. Centraliza a matemática de caixas que estava duplicada em
    /// Hitbox/Hurtbox/BoxCollider/TriggerZone/SceneExit. Reutilizável por componentes e scripts.
    /// </summary>
    public readonly struct Aabb
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        public Aabb(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Constrói a partir do centro e do tamanho (largura×altura).</summary>
        public static Aabb FromCenterSize(Vector2 center, Vector2 size)
        {
            var half = size / 2f;
            return new Aabb(center - half, center + half);
        }

        public Vector2 Center => (Min + Max) / 2f;
        public Vector2 Size => Max - Min;

        /// <summary>Sobreposição estrita: só encostar (bordas coincidentes) não conta.</summary>
        public bool Overlaps(in Aabb o)
            => Min.X < o.Max.X && Max.X > o.Min.X && Min.Y < o.Max.Y && Max.Y > o.Min.Y;

        /// <summary>Sobreposição inclusiva: encostar (bordas coincidentes) conta.</summary>
        public bool Intersects(in Aabb o)
            => Min.X <= o.Max.X && Max.X >= o.Min.X && Min.Y <= o.Max.Y && Max.Y >= o.Min.Y;

        public bool Contains(Vector2 point)
            => point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y;
    }
}
