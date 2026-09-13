using System;
using System.Collections.Generic;
using DreamBit.Engine.Components;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Navigation
{
    /// <summary>
    /// Grade de navegação construída a partir dos tiles sólidos de um <see cref="TilemapRenderer"/>:
    /// converte mundo↔célula e acha caminhos (A*) devolvendo pontos no mundo. Use em scripts de IA:
    /// <c>var grid = NavGrid.FromTilemap(mapa); var caminho = grid.FindPath(inimigo.Pos, alvo.Pos);</c>.
    /// </summary>
    public sealed class NavGrid
    {
        private readonly bool[,] _blocked;
        private readonly int _minX, _minY;
        private readonly int _tileW, _tileH;

        private NavGrid(bool[,] blocked, int minX, int minY, int tileW, int tileH)
        {
            _blocked = blocked;
            _minX = minX; _minY = minY;
            _tileW = tileW; _tileH = tileH;
        }

        public int Width => _blocked.GetLength(0);
        public int Height => _blocked.GetLength(1);
        public bool IsBlocked(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height && _blocked[x, y];

        /// <summary>Monta a grade a partir dos tiles sólidos do tilemap, com uma margem de células
        /// livres ao redor. Retorna null se o tilemap não tem mapa.</summary>
        public static NavGrid? FromTilemap(TilemapRenderer tilemap, int margin = 2)
        {
            if (tilemap == null)
                return null;
            var map = tilemap.Map;
            if (map == null)
                return null;

            // Bounding box das células sólidas (respeitando a camada sólida, se houver).
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            bool any = false;
            foreach (var layer in map.Layers)
            {
                if (!string.IsNullOrEmpty(tilemap.SolidLayer) &&
                    !string.Equals(layer.Name, tilemap.SolidLayer, StringComparison.OrdinalIgnoreCase))
                    continue;
                foreach (var (x, y, _) in layer.Tiles)
                {
                    any = true;
                    if (x < minX) minX = x; if (y < minY) minY = y;
                    if (x > maxX) maxX = x; if (y > maxY) maxY = y;
                }
            }
            if (!any)
                return null;

            minX -= margin; minY -= margin; maxX += margin; maxY += margin;
            int w = maxX - minX + 1, h = maxY - minY + 1;
            var blocked = new bool[w, h];

            foreach (var layer in map.Layers)
            {
                if (!string.IsNullOrEmpty(tilemap.SolidLayer) &&
                    !string.Equals(layer.Name, tilemap.SolidLayer, StringComparison.OrdinalIgnoreCase))
                    continue;
                foreach (var (x, y, _) in layer.Tiles)
                    blocked[x - minX, y - minY] = true;
            }

            return new NavGrid(blocked, minX, minY, map.TileWidth, map.TileHeight);
        }

        /// <summary>Converte um ponto do mundo na célula da grade.</summary>
        public Point WorldToCell(Vector2 world)
            => new((int)Math.Floor(world.X / _tileW) - _minX, (int)Math.Floor(world.Y / _tileH) - _minY);

        /// <summary>Centro (mundo) de uma célula da grade.</summary>
        public Vector2 CellToWorld(Point cell)
            => new((cell.X + _minX) * _tileW + _tileW / 2f, (cell.Y + _minY) * _tileH + _tileH / 2f);

        /// <summary>Acha um caminho entre dois pontos do mundo. Retorna os centros das células
        /// (mundo), do início ao fim; lista vazia se não houver caminho.</summary>
        public List<Vector2> FindPath(Vector2 worldStart, Vector2 worldGoal, bool allowDiagonal = true)
        {
            var cells = Pathfinding.FindPath(_blocked, WorldToCell(worldStart), WorldToCell(worldGoal), allowDiagonal);
            var points = new List<Vector2>(cells.Count);
            foreach (var c in cells)
                points.Add(CellToWorld(c));
            return points;
        }
    }
}
