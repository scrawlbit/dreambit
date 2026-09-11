using System;
using System.IO;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Tilemap;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Desenha (e permite editar) um mapa de tiles no transform do objeto. O mapa pode
    /// vir de um .tmx do Tiled ou ser pintado no editor. Resolve as imagens dos tilesets
    /// (TextureCache).
    /// </summary>
    public sealed class TilemapRenderer : SceneComponent
    {
        private string? _tmxPath;
        private Tilemap.Tilemap? _map;

        public override string DisplayName => "Tilemap";

        /// <summary>Se o mapa foi pintado no editor (deve ser serializado inline).</summary>
        public bool Edited { get; set; }

        public string? TmxPath
        {
            get => _tmxPath;
            set { if (Set(ref _tmxPath, value)) _map = null; }
        }

        /// <summary>O mapa (importa do .tmx sob demanda se ainda não carregado).</summary>
        public Tilemap.Tilemap? Map
        {
            get
            {
                if (_map == null && !string.IsNullOrEmpty(_tmxPath) && File.Exists(_tmxPath))
                {
                    try { _map = TmxImporter.Load(_tmxPath); }
                    catch { _map = null; }
                }
                return _map;
            }
            set => _map = value;
        }

        /// <summary>Converte um ponto de mundo na célula (X, Y) do mapa.</summary>
        public (int X, int Y) WorldToCell(Vector2 world)
        {
            var map = Map;
            int tw = map?.TileWidth ?? 16;
            int th = map?.TileHeight ?? 16;

            var local = Vector2.Transform(world, Matrix.Invert(Owner.Transform.WorldMatrix));
            return ((int)Math.Floor(local.X / tw), (int)Math.Floor(local.Y / th));
        }

        /// <summary>Pinta (ou apaga, se gid=0) um tile na camada de pintura.</summary>
        public void Paint(int cellX, int cellY, int gid)
        {
            var map = Map;
            if (map == null)
                return;

            map.PaintLayer().SetTile(cellX, cellY, gid);
            Edited = true;
        }

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var map = Map;
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
