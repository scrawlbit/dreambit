using System.IO;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Tilemap;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Desenha um mapa de tiles importado do Tiled (.tmx) no transform do objeto.
    /// Resolve as imagens dos tilesets relativas à pasta do .tmx (TextureCache).
    /// </summary>
    public sealed class TilemapRenderer : SceneComponent
    {
        private string? _tmxPath;
        private Tilemap.Tilemap? _map;

        public override string DisplayName => "Tilemap";

        public string? TmxPath
        {
            get => _tmxPath;
            set { if (Set(ref _tmxPath, value)) _map = null; }
        }

        private Tilemap.Tilemap? Map()
        {
            if (_map == null && !string.IsNullOrEmpty(_tmxPath) && File.Exists(_tmxPath))
            {
                try { _map = TmxImporter.Load(_tmxPath); }
                catch { _map = null; }
            }
            return _map;
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var map = Map();
            if (map == null)
                return;

            var device = drawing.SpriteBatch.GraphicsDevice;
            var world = Owner.Transform.WorldMatrix;

            foreach (var layer in map.Layers)
            {
                foreach (var (x, y, gid) in layer.Tiles)
                {
                    var tileset = map.TilesetForGid(gid);
                    if (tileset == null || tileset.Columns <= 0)
                        continue;

                    var texture = TextureCache.Get(device, tileset.ResolvedImagePath);
                    if (texture == null)
                        continue;

                    int local = gid - tileset.FirstGid;
                    int col = local % tileset.Columns;
                    int row = local / tileset.Columns;
                    var source = new Rectangle(col * tileset.TileWidth, row * tileset.TileHeight,
                        tileset.TileWidth, tileset.TileHeight);

                    var localPos = new Vector2(
                        x * map.TileWidth + map.TileWidth / 2f,
                        y * map.TileHeight + map.TileHeight / 2f);
                    var tileWorld = Matrix.CreateTranslation(localPos.X, localPos.Y, 0f) * world;

                    drawing.DrawFrame(tileWorld, new Vector2(tileset.TileWidth, tileset.TileHeight),
                        Color.White, texture, source);
                }
            }
        }
    }
}
