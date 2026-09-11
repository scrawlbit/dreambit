using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using DreamBit.Engine.Tilemap;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class TilemapPaintTests
    {
        [TestMethod]
        public void Paint_DefineEApagaTiles()
        {
            var map = new Tilemap.Tilemap { TileWidth = 16, TileHeight = 16 };
            map.Tilesets.Add(new Tileset { FirstGid = 1, Columns = 4, TileWidth = 16, TileHeight = 16 });
            var tilemap = new TilemapRenderer { Map = map };

            tilemap.Paint(2, 3, 5);
            Assert.AreEqual(5, map.Layers[0].GetTile(2, 3));
            Assert.IsTrue(tilemap.Edited);

            tilemap.Paint(2, 3, 0); // apaga
            Assert.AreEqual(0, map.Layers[0].GetTile(2, 3));
        }

        [TestMethod]
        public void Serializacao_PreservaOMapaPintado()
        {
            var map = new Tilemap.Tilemap { TileWidth = 16, TileHeight = 16 };
            map.Tilesets.Add(new Tileset { FirstGid = 1, ResolvedImagePath = @"C:\assets\ts.png", Columns = 4, TileWidth = 16, TileHeight = 16 });

            var obj = new GameObject("Mapa");
            var tilemap = new TilemapRenderer { Map = map };
            obj.AddComponent(tilemap);
            tilemap.Paint(0, 0, 1);
            tilemap.Paint(5, 2, 7);

            var scene = new Scene();
            scene.Add(obj);

            var path = Path.Combine(Path.GetTempPath(), "dreambit_paint_test.dbscene");
            try
            {
                SceneSerializer.Save(scene, path);
                var loaded = SceneSerializer.Load(path);

                var loadedMap = loaded.Objects[0].Components.OfType<TilemapRenderer>().Single().Map!;
                Assert.AreEqual(1, loadedMap.Tilesets.Count);
                Assert.AreEqual(4, loadedMap.Tilesets[0].Columns);
                Assert.AreEqual(@"C:\assets\ts.png", loadedMap.Tilesets[0].ResolvedImagePath);
                Assert.AreEqual(1, loadedMap.Layers[0].GetTile(0, 0));
                Assert.AreEqual(7, loadedMap.Layers[0].GetTile(5, 2));
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
