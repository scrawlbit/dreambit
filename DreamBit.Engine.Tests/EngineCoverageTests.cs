using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class EngineCoverageTests
    {
        [Fact]
        public void Scene_DefineEReferenciaDeVolta_AoAdicionarERemover()
        {
            var scene = new Scene();
            var obj = new GameObject("o");
            var child = new GameObject("c");
            obj.AddChild(child);

            scene.Add(obj);
            Assert.Same(scene, obj.Scene);
            Assert.Same(scene, child.Scene);

            scene.Remove(obj);
            Assert.Null(obj.Scene);
            Assert.Null(child.Scene);
        }

        [Fact]
        public void CopyFrom_PreservaObjetosELedges()
        {
            var origin = new Scene { Name = "A" };
            origin.Add(new GameObject("x"));
            var ledge = new Ledge();
            ledge.SetPoints(new[] { new Vector2(0, 0), new Vector2(10, 0) });
            origin.AddLedge(ledge);

            var target = new Scene();
            target.CopyFrom(origin);

            Assert.Equal("A", target.Name);
            Assert.Single(target.Objects);
            Assert.Single(target.Ledges);
        }

        [Fact]
        public void Camera_Pan_MoveInversamenteAoZoom()
        {
            var camera = new Camera2D { Position = new Vector2(0, 0), Zoom = 2f };
            camera.Pan(new Vector2(20, 0)); // arrasta a tela 20px

            Assert.Equal(-10f, camera.Position.X, 0.01f); // 20 / zoom(2)
        }

        [Fact]
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

            Assert.True(follower.Transform.Position.X > 0f, "deve se aproximar do alvo");
            Assert.True(follower.Transform.Position.X < 100f);
        }

        [Fact]
        public void GetVisualSize_UsaOSpriteRenderer()
        {
            var obj = new GameObject();
            obj.AddComponent(new SpriteRenderer { Size = new Vector2(200, 50) });

            var size = SceneRenderer.GetVisualSize(obj);
            Assert.Equal(200f, size.X, 0.01f);
            Assert.Equal(50f, size.Y, 0.01f);
        }
    }
}
