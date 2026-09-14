using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class PhysicsRaycastTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / 60.0));

        private static GameObject Wall(Vector2 pos, string name = "Muro")
        {
            var w = new GameObject(name);
            w.Transform.Position = pos;
            w.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Static, Shape = ColliderShape.Box, Width = 40, Height = 200 });
            return w;
        }

        [TestMethod]
        public void RaycastAcertaOCorpo()
        {
            var scene = new Scene();
            var wall = Wall(new Vector2(200, 0));
            scene.Add(wall);
            scene.StartPlay();
            scene.Update(Frame); // inicializa o mundo/corpos

            var hit = scene.Physics.Raycast(new Vector2(0, 0), new Vector2(400, 0));
            Assert.IsNotNull(hit, "o raio deveria atingir o muro");
            Assert.AreSame(wall.Components.OfType<Rigidbody2D>().Single(), hit!.Value.Body);
            Assert.IsTrue(hit.Value.Point.X > 150 && hit.Value.Point.X < 200, $"impacto perto da face esquerda (x={hit.Value.Point.X})");
        }

        [TestMethod]
        public void RaycastSemAlvoRetornaNull()
        {
            var scene = new Scene();
            scene.Add(Wall(new Vector2(200, 500))); // longe do raio
            scene.StartPlay();
            scene.Update(Frame);

            var hit = scene.Physics.Raycast(new Vector2(0, 0), new Vector2(400, 0));
            Assert.IsNull(hit);
        }

        [TestMethod]
        public void RaycastPegaOMaisProximo()
        {
            var scene = new Scene();
            var perto = Wall(new Vector2(100, 0), "Perto");
            var longe = Wall(new Vector2(300, 0), "Longe");
            scene.Add(perto);
            scene.Add(longe);
            scene.StartPlay();
            scene.Update(Frame);

            var hit = scene.Physics.Raycast(new Vector2(0, 0), new Vector2(400, 0));
            Assert.IsNotNull(hit);
            Assert.AreSame(perto.Components.OfType<Rigidbody2D>().Single(), hit!.Value.Body, "o mais próximo");
        }

        [TestMethod]
        public void CamadasDeColisaoNaoInteragem()
        {
            var scene = new Scene();

            var chao = new GameObject("Chao");
            chao.Transform.Position = new Vector2(0, 200);
            // categoria 2, colide só com categoria 2
            chao.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Static, Shape = ColliderShape.Box, Width = 600, Height = 40, CollisionCategory = 2, CollidesWith = 2 });
            scene.Add(chao);

            var caixa = new GameObject("Caixa");
            caixa.Transform.Position = new Vector2(0, 0);
            // categoria 1, colide só com categoria 1 => ignora o chão (cat 2)
            caixa.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Box, Width = 32, Height = 32, CollisionCategory = 1, CollidesWith = 1 });
            scene.Add(caixa);

            scene.StartPlay();
            for (int i = 0; i < 120; i++) scene.Update(Frame);

            Assert.IsTrue(caixa.Transform.Position.Y > 250f, "a caixa atravessa o chão (camadas não interagem)");
        }
    }
}
