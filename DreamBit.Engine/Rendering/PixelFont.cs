using System.Collections.Generic;

namespace DreamBit.Engine.Rendering
{
    /// <summary>
    /// Fonte de bitmap 3×5 embutida (sem pipeline de conteúdo), para desenhar texto/HUD.
    /// Cada glifo são 5 linhas; cada linha usa 3 bits (bit2=esquerda … bit0=direita).
    /// </summary>
    public static class PixelFont
    {
        public const int GlyphWidth = 3;
        public const int GlyphHeight = 5;
        public const int Spacing = 1; // colunas entre caracteres

        private static readonly Dictionary<char, byte[]> Glyphs = new()
        {
            ['0'] = new byte[] { 7, 5, 5, 5, 7 }, ['1'] = new byte[] { 2, 6, 2, 2, 7 },
            ['2'] = new byte[] { 7, 1, 7, 4, 7 }, ['3'] = new byte[] { 7, 1, 7, 1, 7 },
            ['4'] = new byte[] { 5, 5, 7, 1, 1 }, ['5'] = new byte[] { 7, 4, 7, 1, 7 },
            ['6'] = new byte[] { 7, 4, 7, 5, 7 }, ['7'] = new byte[] { 7, 1, 2, 2, 2 },
            ['8'] = new byte[] { 7, 5, 7, 5, 7 }, ['9'] = new byte[] { 7, 5, 7, 1, 7 },
            ['A'] = new byte[] { 2, 5, 7, 5, 5 }, ['B'] = new byte[] { 6, 5, 6, 5, 6 },
            ['C'] = new byte[] { 3, 4, 4, 4, 3 }, ['D'] = new byte[] { 6, 5, 5, 5, 6 },
            ['E'] = new byte[] { 7, 4, 6, 4, 7 }, ['F'] = new byte[] { 7, 4, 6, 4, 4 },
            ['G'] = new byte[] { 3, 4, 5, 5, 3 }, ['H'] = new byte[] { 5, 5, 7, 5, 5 },
            ['I'] = new byte[] { 7, 2, 2, 2, 7 }, ['J'] = new byte[] { 1, 1, 1, 5, 2 },
            ['K'] = new byte[] { 5, 6, 4, 6, 5 }, ['L'] = new byte[] { 4, 4, 4, 4, 7 },
            ['M'] = new byte[] { 5, 7, 7, 5, 5 }, ['N'] = new byte[] { 5, 7, 7, 7, 5 },
            ['O'] = new byte[] { 2, 5, 5, 5, 2 }, ['P'] = new byte[] { 6, 5, 6, 4, 4 },
            ['Q'] = new byte[] { 2, 5, 5, 3, 1 }, ['R'] = new byte[] { 6, 5, 6, 5, 5 },
            ['S'] = new byte[] { 3, 4, 2, 1, 6 }, ['T'] = new byte[] { 7, 2, 2, 2, 2 },
            ['U'] = new byte[] { 5, 5, 5, 5, 7 }, ['V'] = new byte[] { 5, 5, 5, 5, 2 },
            ['W'] = new byte[] { 5, 5, 7, 7, 5 }, ['X'] = new byte[] { 5, 5, 2, 5, 5 },
            ['Y'] = new byte[] { 5, 5, 2, 2, 2 }, ['Z'] = new byte[] { 7, 1, 2, 4, 7 },
            [' '] = new byte[] { 0, 0, 0, 0, 0 }, [':'] = new byte[] { 0, 2, 0, 2, 0 },
            ['.'] = new byte[] { 0, 0, 0, 0, 2 }, ['-'] = new byte[] { 0, 0, 7, 0, 0 },
            ['/'] = new byte[] { 1, 1, 2, 4, 4 }, ['%'] = new byte[] { 5, 1, 2, 4, 5 },
            ['!'] = new byte[] { 2, 2, 2, 0, 2 }, ['?'] = new byte[] { 7, 1, 2, 0, 2 },
            ['+'] = new byte[] { 0, 2, 7, 2, 0 }, ['='] = new byte[] { 0, 7, 0, 7, 0 },
        };

        /// <summary>Linhas do glifo (5 bytes, 3 bits cada). Desconhecido vira espaço.</summary>
        public static byte[] Glyph(char c)
            => Glyphs.TryGetValue(char.ToUpperInvariant(c), out var g) ? g : Glyphs[' '];

        /// <summary>Largura em "pixels de fonte" de um texto (sem escala).</summary>
        public static int MeasureWidth(string text)
            => text.Length == 0 ? 0 : text.Length * (GlyphWidth + Spacing) - Spacing;
    }
}
