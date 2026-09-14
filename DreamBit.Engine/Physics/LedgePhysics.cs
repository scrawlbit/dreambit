using System;
using System.Collections.Generic;
using DreamBit.Engine.Elements;

namespace DreamBit.Engine.Physics
{
    /// <summary>
    /// Colisão simples de personagem contra ledges (polilinhas caminháveis): pousar
    /// sobre a superfície ao cair. Lógica pura, testável sem gráficos.
    /// </summary>
    public static class LedgePhysics
    {
        /// <summary>Altura (Y) da ledge na coluna X, ou NaN se X está fora do span da ledge.
        /// Se vários segmentos cobrem X, retorna o mais alto (menor Y).</summary>
        public static float SurfaceYAt(Ledge ledge, float x)
        {
            float best = float.NaN;

            foreach (var (a, b) in ledge.Segments())
            {
                float minX = Math.Min(a.X, b.X);
                float maxX = Math.Max(a.X, b.X);
                if (x < minX || x > maxX)
                    continue;

                float y;
                if (Math.Abs(b.X - a.X) < 1e-4f)
                    y = Math.Min(a.Y, b.Y); // segmento vertical: usa o topo
                else
                    y = a.Y + (b.Y - a.Y) * (x - a.X) / (b.X - a.X);

                if (float.IsNaN(best) || y < best)
                    best = y;
            }

            return best;
        }

        /// <summary>
        /// Ao cair (pés indo de <paramref name="feetFrom"/> para <paramref name="feetTo"/>),
        /// retorna a superfície de ledge mais alta cruzada nessa coluna X, ou NaN.
        /// </summary>
        public static float FindLanding(IEnumerable<Ledge> ledges, float x, float feetFrom, float feetTo)
        {
            float best = float.NaN;

            foreach (var ledge in ledges)
            {
                float surface = SurfaceYAt(ledge, x);
                if (float.IsNaN(surface))
                    continue;

                if (surface >= feetFrom && surface <= feetTo)
                {
                    if (float.IsNaN(best) || surface < best)
                        best = surface;
                }
            }

            return best;
        }
    }
}
