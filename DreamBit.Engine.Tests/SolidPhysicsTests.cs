using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class SolidPhysicsTests
    {
        private static SolidPhysics.Box Box(float cx, float cy, float w, float h)
            => new(new Vector2(cx - w / 2, cy - h / 2), new Vector2(cx + w / 2, cy + h / 2));

        [TestMethod]
        public void ResolveX_ParaNaParede()
        {
            var solids = new[] { Box(100, 0, 40, 200) }; // parede em x=80..120
            // Jogador (meia-largura 20) indo de x=0 para x=90 — deve parar em 80-20=60.
            float x = SolidPhysics.ResolveX(solids, 0, 90, 0, 20, 20);
            Assert.AreEqual(60f, x, 0.01f);
        }

        [TestMethod]
        public void ResolveY_PousaNoTopo()
        {
            var solids = new[] { Box(0, 100, 200, 40) }; // chão em y=80..120
            var (y, grounded, ceiling) = SolidPhysics.ResolveY(solids, 0, 0, 90, 20, 20); // caindo
            Assert.AreEqual(60f, y, 0.01f); // 80 - 20
            Assert.IsTrue(grounded);
            Assert.IsFalse(ceiling);
        }

        [TestMethod]
        public void ResolveY_BateNoTeto()
        {
            var solids = new[] { Box(0, -100, 200, 40) }; // teto em y=-120..-80
            var (y, grounded, ceiling) = SolidPhysics.ResolveY(solids, 0, 0, -90, 20, 20); // subindo
            Assert.AreEqual(-60f, y, 0.01f); // -80 + 20
            Assert.IsTrue(ceiling);
            Assert.IsFalse(grounded);
        }

        [TestMethod]
        public void Platformer_PousaNoColisorSolido()
        {
            var scene = new Scene();

            var chao = new GameObject("Chao");
            chao.AddComponent(new BoxCollider { Size = new Vector2(400, 40) });
            chao.Transform.Position = new Vector2(0, 200);
            scene.Add(chao);

            var player = new GameObject("Player") { Tag = "Player" };
            player.AddComponent(new PlatformerController { Gravity = 900f, HalfHeight = 24f, HalfWidth = 20f, UseKeyboard = false });
            player.Transform.Position = new Vector2(0, -100);
            scene.Add(player);

            scene.StartPlay();
            for (int i = 0; i < 240; i++)
                scene.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / 60)));

            // topo do chao = 200-20=180; pés do player = y+24 => y ~ 156.
            Assert.AreEqual(156f, player.Transform.Position.Y, 2f, "player deve pousar sobre o colisor sólido");
        }

        [TestMethod]
        public void Serializacao_PreservaColisorEHalfWidth()
        {
            var scene = new Scene();
            var obj = new GameObject("X");
            obj.AddComponent(new BoxCollider { Size = new Vector2(80, 30), Offset = new Vector2(5, -3) });
            obj.AddComponent(new PlatformerController { HalfWidth = 18f });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lo = loaded.Objects.First();
            var box = lo.Components.OfType<BoxCollider>().Single();
            Assert.AreEqual(new Vector2(80, 30), box.Size);
            Assert.AreEqual(new Vector2(5, -3), box.Offset);
            Assert.AreEqual(18f, lo.Components.OfType<PlatformerController>().Single().HalfWidth, 0.01f);
        }
    }
}
