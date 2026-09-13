using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Engine.Tilemap
{
    /// <summary>Um tileset de um mapa Tiled: imagem + faixa de GIDs.</summary>
    public sealed class Tileset
    {
        public int FirstGid { get; set; }
        public string ImageSource { get; set; } = "";
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Columns { get; set; }
        public int TileCount { get; set; }

        /// <summary>Arquivo externo .tsx (quando o tileset não é embutido).</summary>
        public string? Source { get; set; }

        /// <summary>Caminho absoluto da imagem, resolvido no carregamento.</summary>
        public string? ResolvedImagePath { get; set; }
    }

    /// <summary>Uma camada de tiles editável (dicionário esparso célula → GID).</summary>
    public sealed class TileLayer
    {
        private readonly Dictionary<(int X, int Y), int> _tiles = new();

        public string Name { get; set; } = "";

        /// <summary>Se a camada é desenhada (não afeta a colisão sólida).</summary>
        public bool Visible { get; set; } = true;

        public int Count => _tiles.Count;

        /// <summary>Define (ou apaga, se gid=0) o tile numa célula.</summary>
        public void SetTile(int x, int y, int gid)
        {
            if (gid == 0)
                _tiles.Remove((x, y));
            else
                _tiles[(x, y)] = gid;
        }

        public int GetTile(int x, int y) => _tiles.TryGetValue((x, y), out var gid) ? gid : 0;

        public void Clear() => _tiles.Clear();

        /// <summary>Células não vazias (X, Y em tiles, GID).</summary>
        public IEnumerable<(int X, int Y, int Gid)> Tiles =>
            _tiles.Select(kv => (kv.Key.X, kv.Key.Y, kv.Value));
    }

    /// <summary>
    /// Mapa de tiles (importado do Tiled ou editado no editor). Guarda os tilesets e
    /// as camadas; a resolução das imagens usa <see cref="BaseFolder"/>.
    /// </summary>
    public sealed class Tilemap
    {
        public int TileWidth { get; set; } = 16;
        public int TileHeight { get; set; } = 16;
        public string BaseFolder { get; set; } = "";
        public List<Tileset> Tilesets { get; } = new();
        public List<TileLayer> Layers { get; } = new();

        /// <summary>Máscara para remover os bits de flip do GID do Tiled.</summary>
        public const uint GidMask = 0x1FFFFFFF;

        /// <summary>O tileset que contém o GID informado (maior FirstGid &lt;= gid), ou null.</summary>
        public Tileset? TilesetForGid(int gid)
        {
            Tileset? found = null;
            foreach (var tileset in Tilesets)
                if (tileset.FirstGid <= gid && (found == null || tileset.FirstGid > found.FirstGid))
                    found = tileset;
            return found;
        }

        /// <summary>Garante ao menos uma camada e retorna a última (alvo de pintura).</summary>
        public TileLayer PaintLayer()
        {
            if (Layers.Count == 0)
                Layers.Add(new TileLayer { Name = "Camada 1" });
            return Layers[Layers.Count - 1];
        }

        /// <summary>Adiciona uma nova camada no topo (desenhada por último) e a retorna.</summary>
        public TileLayer AddLayer(string? name = null)
        {
            var layer = new TileLayer { Name = string.IsNullOrWhiteSpace(name) ? $"Camada {Layers.Count + 1}" : name };
            Layers.Add(layer);
            return layer;
        }
    }
}
