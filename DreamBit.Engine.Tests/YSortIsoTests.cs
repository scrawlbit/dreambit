using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class YSortIsoTests
    {
        [TestMethod]
        public void YSort_DesenhaMaisAbaixoNaFrente()
        {
            var scene = new Scene();
            var back = new GameObject("Fundo"); back.Transform.Position = new Vector2(0, 100); back.AddComponent(new YSort());
            var front = new GameObject("Frente"); front.Transform.Position = new Vector2(0, 300); front.AddComponent(new YSort());
            scene.Add(front); // adicionado antes, mas Y maior => desenha depois (na frente)
            scene.Add(back);

            var order = scene.VisibleInDrawOrder().ToList();
            Assert.IsTrue(order.IndexOf(back) < order.IndexOf(front), "quem tem Y menor desenha primeiro (atrás)");

            back.Transform.Position = new Vector2(0, 500); // agora atrás vira frente
            order = scene.VisibleInDrawOrder().ToList();
            Assert.IsTrue(order.IndexOf(front) < order.IndexOf(back), "reordena por Y ao mover");
        }

        [TestMethod]
        public void Isometrico_MapeiaCelulaMundoEVolta()
        {
            var map = new Tilemap.Tilemap { TileWidth = 64, TileHeight = 32 };
            var o = new GameObject("Iso");
            var tm = new TilemapRenderer { Map = map, Orientation = TileOrientation.Isometric };
            o.AddComponent(tm);
            new Scene().Add(o);

            // Célula (2,1): ((2-1)*32, (2+1)*16) = (32, 48)
            var c = tm.CellToLocalCenter(2, 1);
            Assert.AreEqual(32f, c.X, 0.001f);
            Assert.AreEqual(48f, c.Y, 0.001f);

            // Round-trip mundo->célula.
            var cell = tm.WorldToCell(tm.CellToLocalCenter(3, 2));
            Assert.AreEqual(3, cell.X);
            Assert.AreEqual(2, cell.Y);
        }

        [TestMethod]
        public void Serializacao_RoundTrip_YSortEOrientacao()
        {
            var scene = new Scene();
            var o = new GameObject("O");
            o.AddComponent(new YSort { Offset = 24f });
            var map = new Tilemap.Tilemap { TileWidth = 64, TileHeight = 32 };
            o.AddComponent(new TilemapRenderer { Map = map, Edited = true, Orientation = TileOrientation.Isometric });
            scene.Add(o);

            var e = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene)).Objects.First();
            Assert.AreEqual(24f, e.Components.OfType<YSort>().Single().Offset);
            Assert.AreEqual(TileOrientation.Isometric, e.Components.OfType<TilemapRenderer>().Single().Orientation);
        }
    }
}
