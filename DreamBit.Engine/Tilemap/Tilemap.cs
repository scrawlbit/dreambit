using System.Collections.Generic;

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

    /// <summary>Uma camada de tiles: apenas as células não vazias (X, Y em tiles, GID).</summary>
    public sealed class TileLayer
    {
        public string Name { get; set; } = "";
        public List<(int X, int Y, int Gid)> Tiles { get; } = new();
    }

    /// <summary>
    /// Mapa de tiles importado do formato Tiled (.tmx). Guarda os tilesets e as
    /// camadas; a resolução das imagens usa <see cref="BaseFolder"/>.
    /// </summary>
    public sealed class Tilemap
    {
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
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
    }
}
