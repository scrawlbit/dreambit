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
    public class PhysicsJointTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / 60.0));

        [TestMethod]
        public void JointDistancia_SeguraOCorpoContraAGravidade()
        {
            var scene = new Scene();
            var anchor = new GameObject("Anchor") { Tag = "anchor" };
            anchor.Transform.Position = new Vector2(0, 0);
            anchor.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Static, Shape = ColliderShape.Circle, Radius = 8 });
            scene.Add(anchor);

            var ball = new GameObject("Bola");
            ball.Transform.Position = new Vector2(0, 100);
            ball.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Circle, Radius = 12 });
            ball.AddComponent(new Joint2D { Kind = Joint2DKind.Distance, ConnectedTag = "anchor" });
            scene.Add(ball);

            scene.StartPlay();
            for (int i = 0; i < 180; i++) scene.Update(Frame); // 3 s

            // Sem a junta a bola cairia ~metros; com a junta fica presa a ~100px do apoio.
            float dist = Vector2.Distance(ball.Transform.Position, anchor.Transform.Position);
            Assert.IsTrue(dist < 170f, $"a junta segura a bola (dist={dist:0})");
            Assert.IsTrue(ball.Transform.Position.Y < 200f, $"não caiu livremente (y={ball.Transform.Position.Y:0})");
        }

        [TestMethod]
        public void JointPontoFixo_SemTag_PrendeNoMundo()
        {
            var scene = new Scene();
            var ball = new GameObject("Preso");
            ball.Transform.Position = new Vector2(50, 50);
            ball.AddComponent(new Rigidbody2D { Kind = RigidbodyKind.Dynamic, Shape = ColliderShape.Box, Width = 20, Height = 20 });
            ball.AddComponent(new Joint2D { Kind = Joint2DKind.Weld }); // sem tag = solda num ponto fixo
            scene.Add(ball);

            scene.StartPlay();
            for (int i = 0; i < 120; i++) scene.Update(Frame);

            // Soldado ao ponto de origem: praticamente não se move apesar da gravidade.
            Assert.IsTrue(Vector2.Distance(ball.Transform.Position, new Vector2(50, 50)) < 20f,
                $"weld prende no lugar (pos={ball.Transform.Position})");
        }

        [TestMethod]
        public void Serializacao_RoundTrip_Joint()
        {
            var scene = new Scene();
            var o = new GameObject("J");
            o.AddComponent(new Rigidbody2D());
            o.AddComponent(new Joint2D { Kind = Joint2DKind.Revolute, ConnectedTag = "hub", Anchor = new Vector2(10, -4), Frequency = 3f, DampingRatio = 0.7f, CollideConnected = true });
            scene.Add(o);

            var e = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene)).Objects.First();
            var j = e.Components.OfType<Joint2D>().Single();
            Assert.AreEqual(Joint2DKind.Revolute, j.Kind);
            Assert.AreEqual("hub", j.ConnectedTag);
            Assert.AreEqual(new Vector2(10, -4), j.Anchor);
            Assert.AreEqual(3f, j.Frequency);
            Assert.IsTrue(j.CollideConnected);
        }
    }
}
