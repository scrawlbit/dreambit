using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Physics
{
    /// <summary>
    /// Resolução de colisão AABB sólida (por eixo) contra caixas fixas. Complementa a
    /// LedgePhysics (one-way): aqui a caixa bloqueia por todos os lados. Lógica pura,
    /// testável, sem dependência de componentes.
    /// </summary>
    public static class SolidPhysics
    {
        public readonly record struct Box(Vector2 Min, Vector2 Max);

        private static bool Overlap(Vector2 aMin, Vector2 aMax, Box b)
            => aMin.X < b.Max.X && aMax.X > b.Min.X && aMin.Y < b.Max.Y && aMax.Y > b.Min.Y;

        /// <summary>Resolve o movimento horizontal do centro (halfW×halfH) de fromX a toX na altura y.
        /// Retorna o X resolvido (parado na face do sólido, se colidir).</summary>
        public static float ResolveX(IReadOnlyList<Box> solids, float fromX, float toX, float y, float halfW, float halfH)
        {
            float x = toX;
            for (int i = 0; i < solids.Count; i++)
            {
                var min = new Vector2(x - halfW, y - halfH);
                var max = new Vector2(x + halfW, y + halfH);
                if (!Overlap(min, max, solids[i]))
                    continue;

                if (toX > fromX) x = solids[i].Min.X - halfW;      // indo p/ direita: para na face esquerda
                else if (toX < fromX) x = solids[i].Max.X + halfW; // indo p/ esquerda: para na face direita
            }
            return x;
        }

        /// <summary>Resolve o movimento vertical do centro de fromY a toY na coluna x.
        /// Retorna Y resolvido e se pousou no chão (topo) ou bateu no teto.</summary>
        public static (float Y, bool Grounded, bool Ceiling) ResolveY(
            IReadOnlyList<Box> solids, float x, float fromY, float toY, float halfW, float halfH)
        {
            float y = toY;
            bool grounded = false, ceiling = false;
            for (int i = 0; i < solids.Count; i++)
            {
                var min = new Vector2(x - halfW, y - halfH);
                var max = new Vector2(x + halfW, y + halfH);
                if (!Overlap(min, max, solids[i]))
                    continue;

                if (toY > fromY) { y = solids[i].Min.Y - halfH; grounded = true; } // caindo: pousa no topo
                else if (toY < fromY) { y = solids[i].Max.Y + halfH; ceiling = true; } // subindo: bate no teto
            }
            return (y, grounded, ceiling);
        }
    }
}
