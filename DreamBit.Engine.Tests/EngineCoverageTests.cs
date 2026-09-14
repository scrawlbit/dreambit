using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class EngineCoverageTests
    {
        [TestMethod]
        public void Scene_DefineEReferenciaDeVolta_AoAdicionarERemover()
        {
            var scene = new Scene();
            var obj = new GameObject("o");
            var child = new GameObject("c");
            obj.AddChild(child);

            scene.Add(obj);
            Assert.AreSame(scene, obj.Scene);
            Assert.AreSame(scene, child.Scene, "a cena propaga para os filhos");

            scene.Remove(obj);
            Assert.IsNull(obj.Scene);
            Assert.IsNull(child.Scene);
        }

        [TestMethod]
        public void CopyFrom_PreservaObjetosELedges()
        {
            var origin = new Scene { Name = "A" };
            origin.Add(new GameObject("x"));
            var ledge = new Ledge();
            ledge.SetPoints(new[] { new Vector2(0, 0), new Vector2(10, 0) });
            origin.AddLedge(ledge);

            var target = new Scene();
            target.CopyFrom(origin);

            Assert.AreEqual("A", target.Name);
            Assert.AreEqual(1, target.Objects.Count);
            Assert.AreEqual(1, target.Ledges.Count);
        }

        [TestMethod]
        public void Camera_Pan_MoveInversamenteAoZoom()
        {
            var camera = new Camera2D { Position = new Vector2(0, 0), Zoom = 2f };
            camera.Pan(new Vector2(20, 0)); // arrasta a tela 20px

            Assert.AreEqual(-10f, camera.Position.X, 0.01f); // 20 / zoom(2)
        }

        [TestMethod]
        public void FollowTarget_AproximaDoAlvo()
        {
            var scene = new Scene();
            var follower = new GameObject("f");
            follower.AddComponent(new FollowTarget { Speed = 10f });
            follower.Transform.Position = new Vector2(0, 0);
            scene.Add(follower);

            var target = new GameObject("t");
            target.Transform.Position = new Vector2(100, 0);
            scene.Add(target);

            follower.Components.OfType<FollowTarget>().Single().TargetId = target.Id;

            scene.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.05))); // t = 0.5

            Assert.IsTrue(follower.Transform.Position.X > 0f, "deve se aproximar do alvo");
            Assert.IsTrue(follower.Transform.Position.X < 100f);
        }

        [TestMethod]
        public void GetVisualSize_UsaOSpriteRenderer()
        {
            var obj = new GameObject();
            obj.AddComponent(new SpriteRenderer { Size = new Vector2(200, 50) });

            var size = SceneRenderer.GetVisualSize(obj);
            Assert.AreEqual(200f, size.X, 0.01f);
            Assert.AreEqual(50f, size.Y, 0.01f);
        }
    }
}
