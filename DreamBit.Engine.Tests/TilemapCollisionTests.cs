using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class TilemapCollisionTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        private static GameObject SolidGround()
        {
            var obj = new GameObject("Chao");
            var map = new Tilemap.Tilemap { TileWidth = 32, TileHeight = 32 };
            var layer = new Tilemap.TileLayer { Name = "Solido" };
            for (int x = -3; x <= 3; x++)
                layer.SetTile(x, 0, 1); // linha de tiles em y=0 (mundo 0..32)
            map.Layers.Add(layer);
            obj.AddComponent(new TilemapRenderer { Map = map, Solid = true });
            return obj;
        }

        [TestMethod]
        public void SolidBoxes_UmaCaixaPorCelula()
        {
            var ground = SolidGround();
            ground.Transform.Position = Vector2.Zero;
            var tilemap = ground.Components.OfType<TilemapRenderer>().Single();

            var boxes = tilemap.SolidBoxes().ToList();
            Assert.AreEqual(7, boxes.Count);

            // célula x=0 => mundo [0,0]..[32,32]
            var cell0 = boxes.First(b => Math.Abs(b.Min.X) < 0.01f);
            Assert.AreEqual(new Vector2(0, 0), cell0.Min);
            Assert.AreEqual(new Vector2(32, 32), cell0.Max);
        }

        [TestMethod]
        public void SolidFalse_SemCaixas()
        {
            var ground = SolidGround();
            ground.Components.OfType<TilemapRenderer>().Single().Solid = false;
            Assert.AreEqual(0, ground.Components.OfType<TilemapRenderer>().Single().SolidBoxes().Count());
        }

        [TestMethod]
        public void Platformer_PousaSobreOsTiles()
        {
            var scene = new Scene();
            var ground = SolidGround(); // topo dos tiles em y=0
            ground.Transform.Position = Vector2.Zero;
            scene.Add(ground);

            var hero = new GameObject("Heroi");
            hero.Transform.Position = new Vector2(0, -100);
            hero.AddComponent(new PlatformerController { Gravity = 900f, HalfHeight = 10f, HalfWidth = 8f, UseKeyboard = false, HorizontalSpeed = 0f });
            scene.Add(hero);

            scene.StartPlay();
            for (int i = 0; i < 120; i++)
                scene.Update(Frame);

            // pés (y + HalfHeight) devem parar na superfície (topo do tile em y=0).
            Assert.AreEqual(-10f, hero.Transform.Position.Y, 1.5f, "personagem pousa sobre os tiles sólidos");
        }

        [TestMethod]
        public void CamadaSolidaRestringe()
        {
            var obj = new GameObject("Mapa");
            var map = new Tilemap.Tilemap { TileWidth = 16, TileHeight = 16 };
            var fundo = new Tilemap.TileLayer { Name = "Fundo" };
            fundo.SetTile(0, 0, 1);
            var solido = new Tilemap.TileLayer { Name = "Colisao" };
            solido.SetTile(5, 5, 1);
            map.Layers.Add(fundo);
            map.Layers.Add(solido);
            obj.AddComponent(new TilemapRenderer { Map = map, Solid = true, SolidLayer = "Colisao" });

            var boxes = obj.Components.OfType<TilemapRenderer>().Single().SolidBoxes().ToList();
            Assert.AreEqual(1, boxes.Count, "só a camada Colisao conta");
            Assert.AreEqual(new Vector2(80, 80), boxes[0].Min);
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Mapa");
            var map = new Tilemap.Tilemap { TileWidth = 16, TileHeight = 16 };
            map.PaintLayer().SetTile(0, 0, 1);
            obj.AddComponent(new TilemapRenderer { Map = map, Edited = true, Solid = true, SolidLayer = "Pintura" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var t = loaded.Objects.First().Components.OfType<TilemapRenderer>().Single();
            Assert.IsTrue(t.Solid);
            Assert.AreEqual("Pintura", t.SolidLayer);
        }
    }
}
