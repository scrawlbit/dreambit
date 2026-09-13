using System.Collections.Generic;

namespace DreamBit.Engine.Tilemap
{
    /// <summary>
    /// Autotiling por vizinhança (esquema de 4 bits / 16 tiles): dado quais células estão
    /// "preenchidas" (parede/chão), escolhe a variante de tile certa conforme os vizinhos
    /// cardeais (N/L/S/O). O tileset deve ter 16 tiles onde o id local = máscara (bit 0 = N,
    /// 1 = Leste, 2 = Sul, 3 = Oeste). Equivale ao autotile do Tiled/Godot (borda de 4 bits).
    /// </summary>
    public static class Autotile
    {
        /// <summary>Máscara 4-bit dos vizinhos preenchidos de (x,y): N=1, L=2, S=4, O=8.</summary>
        public static int Mask4(HashSet<(int X, int Y)> filled, int x, int y)
        {
            int mask = 0;
            if (filled.Contains((x, y - 1))) mask |= 1; // Norte
            if (filled.Contains((x + 1, y))) mask |= 2; // Leste
            if (filled.Contains((x, y + 1))) mask |= 4; // Sul
            if (filled.Contains((x - 1, y))) mask |= 8; // Oeste
            return mask;
        }

        /// <summary>Preenche a camada com as variantes autotile: cada célula preenchida recebe
        /// <c>firstGid + máscara(0..15)</c>. As células não preenchidas ficam vazias.</summary>
        public static void Apply(TileLayer layer, IEnumerable<(int X, int Y)> filledCells, int firstGid)
        {
            var filled = new HashSet<(int X, int Y)>(filledCells);
            foreach (var (x, y) in filled)
                layer.SetTile(x, y, firstGid + Mask4(filled, x, y));
        }
    }
}
