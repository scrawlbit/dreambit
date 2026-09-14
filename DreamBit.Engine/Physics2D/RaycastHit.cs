using DreamBit.Engine.Components;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Physics2D
{
    /// <summary>Resultado de um raycast na física: onde o raio bateu e em qual corpo.</summary>
    /// <param name="Point">Ponto de impacto (pixels).</param>
    /// <param name="Normal">Normal da superfície no impacto.</param>
    /// <param name="Body">Corpo atingido (pode ser null se o corpo não tem Rigidbody2D associado).</param>
    /// <param name="Fraction">Fração ao longo do raio [0..1] onde bateu.</param>
    public readonly record struct RaycastHit(Vector2 Point, Vector2 Normal, Rigidbody2D? Body, float Fraction);
}
