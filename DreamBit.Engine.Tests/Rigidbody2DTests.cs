using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class Rigidbody2DTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / 60.0));

        [TestMethod]
        public void CorpoDinamicoCaiPelaGravidade()
        {
            var scene = new Scene();
            var caixa = new GameObject("Caixa");
            caixa.Transform.Position = new Vector2(0, 0);
            caixa.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Box, Width = 32, Height = 32 });
            scene.Add(caixa);

            scene.StartPlay();
            float y0 = caixa.Transform.Position.Y;
            for (int i = 0; i < 60; i++)
                scene.Update(Frame); // ~1s

            Assert.IsTrue(caixa.Transform.Position.Y > y0 + 50f,
                $"a caixa deveria cair (y0={y0}, y={caixa.Transform.Position.Y})");
        }

        [TestMethod]
        public void DinamicoPousaSobreEstatico()
        {
            var scene = new Scene();

            var chao = new GameObject("Chao");
            chao.Transform.Position = new Vector2(0, 300);
            chao.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Static, Shape = ColliderShape.Box, Width = 600, Height = 40 });
            scene.Add(chao);

            var caixa = new GameObject("Caixa");
            caixa.Transform.Position = new Vector2(0, 0);
            caixa.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Box, Width = 32, Height = 32, FixedRotation = true });
            scene.Add(caixa);

            scene.StartPlay();
            for (int i = 0; i < 240; i++) // 4s para assentar
                scene.Update(Frame);

            // topo do chão em y=300-20=280; centro da caixa deve parar ~ 280-16 = 264.
            Assert.AreEqual(264f, caixa.Transform.Position.Y, 6f,
                $"a caixa deveria assentar sobre o chão (y={caixa.Transform.Position.Y})");
        }

        [TestMethod]
        public void ImpulsoAlteraVelocidade()
        {
            var scene = new Scene();
            var bola = new GameObject("Bola");
            bola.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Circle, Radius = 12 });
            scene.Add(bola);

            scene.StartPlay();
            var rb = bola.Components.OfType<Rigidbody2D>().Single();
            rb.ApplyImpulse(new Vector2(500, 0));
            scene.Update(Frame);

            Assert.IsTrue(rb.LinearVelocity.X > 10f, $"impulso deveria dar velocidade em X (vx={rb.LinearVelocity.X})");
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Bola");
            obj.AddComponent(new Rigidbody2D
            {
                Kind = RigidbodyKind.Kinematic, Shape = ColliderShape.Circle, Radius = 15,
                Density = 2f, Friction = 0.5f, Restitution = 0.8f, FixedRotation = true
            });
            scene.Add(obj);

            var loaded = DreamBit.Engine.Serialization.SceneSerializer.LoadFromString(
                DreamBit.Engine.Serialization.SceneSerializer.SaveToString(scene));
            var rb = loaded.Objects.First().Components.OfType<Rigidbody2D>().Single();
            Assert.AreEqual(RigidbodyKind.Kinematic, rb.Kind);
            Assert.AreEqual(ColliderShape.Circle, rb.Shape);
            Assert.AreEqual(15f, rb.Radius);
            Assert.AreEqual(2f, rb.Density);
            Assert.AreEqual(0.8f, rb.Restitution, 0.001f);
            Assert.IsTrue(rb.FixedRotation);
        }

        [TestMethod]
        public void StaticNaoCai()
        {
            var scene = new Scene();
            var plataforma = new GameObject("Plat");
            plataforma.Transform.Position = new Vector2(10, 20);
            plataforma.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Static, Shape = ColliderShape.Box, Width = 100, Height = 20 });
            scene.Add(plataforma);

            scene.StartPlay();
            for (int i = 0; i < 60; i++)
                scene.Update(Frame);

            Assert.AreEqual(20f, plataforma.Transform.Position.Y, 0.5f, "corpo estático não se move");
        }
    }
}
