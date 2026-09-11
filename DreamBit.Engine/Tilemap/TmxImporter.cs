using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace DreamBit.Engine.Tilemap
{
    /// <summary>
    /// Importa mapas do Tiled (.tmx) — ortogonais; encoding CSV e Base64 (zlib/gzip);
    /// mapas finitos e infinitos (chunks); tilesets embutidos e externos (.tsx).
    /// </summary>
    public static class TmxImporter
    {
        public static Tilemap Load(string tmxPath)
        {
            var doc = XDocument.Load(tmxPath);
            string baseFolder = Path.GetDirectoryName(Path.GetFullPath(tmxPath)) ?? "";
            var map = Parse(doc, baseFolder);
            return map;
        }

        /// <summary>Parse do XML. Com <paramref name="baseFolder"/> resolve .tsx e imagens.</summary>
        public static Tilemap Parse(XDocument doc, string? baseFolder = null)
        {
            var root = doc.Root ?? throw new InvalidDataException("TMX sem elemento raiz.");
            var map = new Tilemap
            {
                TileWidth = (int?)root.Attribute("tilewidth") ?? 16,
                TileHeight = (int?)root.Attribute("tileheight") ?? 16,
                BaseFolder = baseFolder ?? ""
            };

            foreach (var ts in root.Elements("tileset"))
                map.Tilesets.Add(ParseTileset(ts, map, baseFolder));

            foreach (var layerElement in root.Elements("layer"))
                map.Layers.Add(ParseLayer(layerElement));

            return map;
        }

        private static Tileset ParseTileset(XElement ts, Tilemap map, string? baseFolder)
        {
            var tileset = new Tileset
            {
                FirstGid = (int?)ts.Attribute("firstgid") ?? 1,
                Source = (string?)ts.Attribute("source")
            };

            // Tileset externo (.tsx): carrega a definição do arquivo referenciado.
            XElement definition = ts;
            string imageFolder = baseFolder ?? "";
            if (tileset.Source != null && baseFolder != null)
            {
                try
                {
                    string tsxPath = Path.Combine(baseFolder, tileset.Source);
                    definition = XDocument.Load(tsxPath).Root ?? ts;
                    imageFolder = Path.GetDirectoryName(Path.GetFullPath(tsxPath)) ?? baseFolder;
                }
                catch { definition = ts; }
            }

            tileset.TileWidth = (int?)definition.Attribute("tilewidth") ?? map.TileWidth;
            tileset.TileHeight = (int?)definition.Attribute("tileheight") ?? map.TileHeight;
            tileset.Columns = (int?)definition.Attribute("columns") ?? 1;
            tileset.TileCount = (int?)definition.Attribute("tilecount") ?? 0;

            var image = definition.Element("image");
            tileset.ImageSource = (string?)image?.Attribute("source") ?? "";

            if (!string.IsNullOrEmpty(tileset.ImageSource) && !string.IsNullOrEmpty(imageFolder))
                tileset.ResolvedImagePath = Path.Combine(imageFolder, tileset.ImageSource);

            return tileset;
        }

        private static TileLayer ParseLayer(XElement layerElement)
        {
            var layer = new TileLayer { Name = (string?)layerElement.Attribute("name") ?? "" };
            var data = layerElement.Element("data");
            if (data == null)
                return layer;

            string encoding = (string?)data.Attribute("encoding") ?? "csv";
            string? compression = (string?)data.Attribute("compression");

            var chunks = data.Elements("chunk").ToList();
            if (chunks.Count > 0)
            {
                foreach (var chunk in chunks)
                {
                    int cx = (int?)chunk.Attribute("x") ?? 0;
                    int cy = (int?)chunk.Attribute("y") ?? 0;
                    int cw = (int?)chunk.Attribute("width") ?? 0;
                    Place(DecodeGids(chunk.Value, encoding, compression), cw, cx, cy, layer);
                }
            }
            else
            {
                int width = (int?)layerElement.Attribute("width") ?? 0;
                Place(DecodeGids(data.Value, encoding, compression), width, 0, 0, layer);
            }

            return layer;
        }

        private static void Place(IEnumerable<int> gids, int width, int originX, int originY, TileLayer layer)
        {
            if (width <= 0)
                return;

            int index = 0;
            foreach (int gid in gids)
            {
                if (gid != 0)
                    layer.Tiles.Add((originX + index % width, originY + index / width, gid));
                index++;
            }
        }

        private static IEnumerable<int> DecodeGids(string content, string encoding, string? compression)
        {
            if (encoding == "base64")
                return DecodeBase64(content, compression);

            // CSV
            return content
                .Split(new[] { ',', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => uint.TryParse(t, out uint v) ? (int)(v & Tilemap.GidMask) : 0);
        }

        private static IEnumerable<int> DecodeBase64(string content, string? compression)
        {
            byte[] raw = Convert.FromBase64String(content.Trim());
            byte[] bytes = compression switch
            {
                "zlib" => Inflate(new ZLibStream(new MemoryStream(raw), CompressionMode.Decompress)),
                "gzip" => Inflate(new GZipStream(new MemoryStream(raw), CompressionMode.Decompress)),
                _ => raw
            };

            var gids = new List<int>(bytes.Length / 4);
            for (int i = 0; i + 3 < bytes.Length; i += 4)
            {
                uint gid = (uint)(bytes[i] | bytes[i + 1] << 8 | bytes[i + 2] << 16 | bytes[i + 3] << 24);
                gids.Add((int)(gid & Tilemap.GidMask));
            }
            return gids;
        }

        private static byte[] Inflate(Stream stream)
        {
            using (stream)
            using (var output = new MemoryStream())
            {
                stream.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}
