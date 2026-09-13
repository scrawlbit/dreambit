using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Chroma key (fundo removível): detecta a cor de fundo de uma imagem e torna transparentes
    /// os pixels próximos dela — como um "green screen" para sprites. Algoritmo puro sobre um
    /// vetor de cores (testável, sem GPU). O <see cref="TextureCache"/> usa isto ao carregar.
    /// </summary>
    public static class ChromaKey
    {
        /// <summary>Detecta a cor de fundo: a mais frequente entre os pixels da borda da imagem.</summary>
        public static Color DetectBackground(Color[] pixels, int width, int height)
        {
            if (pixels == null || width <= 0 || height <= 0 || pixels.Length < width * height)
                return Color.Transparent;

            var counts = new Dictionary<uint, int>();
            void Tally(int x, int y)
            {
                var c = pixels[y * width + x];
                uint key = (uint)(c.R << 16 | c.G << 8 | c.B);
                counts.TryGetValue(key, out int n);
                counts[key] = n + 1;
            }

            for (int x = 0; x < width; x++) { Tally(x, 0); Tally(x, height - 1); }
            for (int y = 0; y < height; y++) { Tally(0, y); Tally(width - 1, y); }

            uint best = 0; int bestCount = -1;
            foreach (var kv in counts)
                if (kv.Value > bestCount) { bestCount = kv.Value; best = kv.Key; }

            return new Color((byte)(best >> 16), (byte)(best >> 8), (byte)best);
        }

        /// <summary>True se a cor está dentro da <paramref name="tolerance"/> da cor-chave
        /// (soma das diferenças de R+G+B).</summary>
        public static bool Matches(Color c, Color chroma, int tolerance)
        {
            int d = System.Math.Abs(c.R - chroma.R) + System.Math.Abs(c.G - chroma.G) + System.Math.Abs(c.B - chroma.B);
            return d <= tolerance;
        }

        /// <summary>Torna transparentes (no lugar) os pixels que casam com a cor-chave.
        /// Retorna quantos pixels foram removidos.</summary>
        public static int Apply(Color[] pixels, Color chroma, int tolerance)
        {
            int removed = 0;
            for (int i = 0; i < pixels.Length; i++)
                if (Matches(pixels[i], chroma, tolerance))
                {
                    pixels[i] = Color.Transparent; // (0,0,0,0), evita halo em alpha premultiplicado
                    removed++;
                }
            return removed;
        }
    }
}
