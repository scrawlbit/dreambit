using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using DreamBit.Engine.Tilemap;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class TileFeaturesTests
    {
        [TestMethod]
        public void TileAnimado_ResolveQuadroPeloTempo()
        {
            var ts = new Tileset { FirstGid = 1, Columns = 4, TileWidth = 16, TileHeight = 16 };
            ts.Animations[0] = new List<TileAnimationFrame>
            {
                new(0, 100), new(1, 100), new(2, 100) // 3 quadros, 300ms no total
            };

            Assert.AreEqual(0, ts.ResolveLocalTile(0, 50));
            Assert.AreEqual(1, ts.ResolveLocalTile(0, 150));
            Assert.AreEqual(2, ts.ResolveLocalTile(0, 250));
            Assert.AreEqual(0, ts.ResolveLocalTile(0, 350), "cicla ao passar do total");
            Assert.AreEqual(5, ts.ResolveLocalTile(5, 999), "tile sem animação é ele mesmo");
        }

        [TestMethod]
        public void Autotile_Mask4_ContaVizinhos()
        {
            var filled = new HashSet<(int, int)> { (1, 1), (1, 0), (2, 1), (1, 2), (0, 1) };
            // centro (1,1) tem os 4 vizinhos -> N|L|S|O = 1+2+4+8 = 15
            Assert.AreEqual(15, Autotile.Mask4(filled, 1, 1));
            // (1,0) topo: só vizinho ao Sul (1,1) -> 4
            Assert.AreEqual(4, Autotile.Mask4(filled, 1, 0));
            // (0,1) esquerda: só vizinho a Leste (1,1) -> 2
            Assert.AreEqual(2, Autotile.Mask4(filled, 0, 1));
        }

        [TestMethod]
        public void Autotile_Apply_PreencheVariantes()
        {
            var layer = new TileLayer { Name = "Parede" };
            var cells = new[] { (0, 0), (1, 0), (0, 1), (1, 1) }; // bloco 2x2
            Autotile.Apply(layer, cells, firstGid: 100);

            // (0,0): vizinhos preenchidos = Leste (1,0) e Sul (0,1) -> 2+4 = 6 -> gid 106
            Assert.AreEqual(106, layer.GetTile(0, 0));
            // (1,1): vizinhos = Norte (1,0) e Oeste (0,1) -> 1+8 = 9 -> gid 109
            Assert.AreEqual(109, layer.GetTile(1, 1));
            Assert.AreEqual(4, layer.Count);
        }

        [TestMethod]
        public void Serializacao_RoundTrip_TileAnimado()
        {
            var map = new Tilemap.Tilemap { TileWidth = 16, TileHeight = 16 };
            var ts = new Tileset { FirstGid = 1, Columns = 4, TileWidth = 16, TileHeight = 16, ResolvedImagePath = "agua.png" };
            ts.Animations[0] = new List<TileAnimationFrame> { new(0, 120), new(1, 120) };
            map.Tilesets.Add(ts);
            map.PaintLayer().SetTile(0, 0, 1);

            var scene = new Scene();
            var o = new GameObject("Agua");
            o.AddComponent(new TilemapRenderer { Map = map, Edited = true });
            scene.Add(o);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lmap = loaded.Objects.First().Components.OfType<TilemapRenderer>().Single().Map!;
            var lts = lmap.Tilesets.Single();
            Assert.IsTrue(lts.Animations.ContainsKey(0));
            Assert.AreEqual(2, lts.Animations[0].Count);
            Assert.AreEqual(120, lts.Animations[0][1].DurationMs);
        }
    }
}
