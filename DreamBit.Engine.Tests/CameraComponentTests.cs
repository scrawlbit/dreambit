using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class CameraComponentTests
    {
        private static (Scene scene, GameObject player, CameraComponent cam) Make()
        {
            var scene = new Scene();
            var player = new GameObject("P") { Tag = "Player" };
            scene.Add(player);
            var camObj = new GameObject("Cam");
            var cam = new CameraComponent { TargetTag = "Player", SmoothTime = 0f };
            camObj.AddComponent(cam);
            scene.Add(camObj);
            return (scene, player, cam);
        }

        [Fact]
        public void Deadzone_NaoMoveDentro_MoveFora()
        {
            var (_, player, cam) = Make();
            cam.DeadzoneWidth = 100f; cam.DeadzoneHeight = 100f;
            var camera = new Camera2D { Position = Vector2.Zero };

            player.Transform.Position = new Vector2(30, 0); // dentro da deadzone (±50)
            cam.DriveCamera(camera, 0.016f, 800, 600);
            Assert.Equal(0f, camera.Position.X, 0.01f);

            player.Transform.Position = new Vector2(200, 0); // fora
            cam.DriveCamera(camera, 0.016f, 800, 600);
            Assert.Equal(150f, camera.Position.X, 0.01f);
        }

        [Fact]
        public void Bounds_MantemAVistaDentro()
        {
            var (_, player, cam) = Make();
            cam.DeadzoneWidth = 0f; cam.DeadzoneHeight = 0f; cam.Zoom = 1f;
            cam.UseBounds = true;
            cam.BoundsMin = new Vector2(-500, -500);
            cam.BoundsMax = new Vector2(500, 500);
            var camera = new Camera2D { Position = Vector2.Zero };

            player.Transform.Position = new Vector2(1000, 0); // muito à direita
            cam.DriveCamera(camera, 1f, 800, 600); // meia-vista = 400

            // clamp: max 500 - 400 = 100
            Assert.Equal(100f, camera.Position.X, 0.5f);
        }

        [Fact]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Cam");
            obj.AddComponent(new CameraComponent
            {
                TargetTag = "Hero", DeadzoneWidth = 200, DeadzoneHeight = 90,
                SmoothTime = 0.2f, Zoom = 1.5f, UseBounds = true,
                BoundsMin = new Vector2(-100, -50), BoundsMax = new Vector2(100, 50)
            });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var cam = loaded.Objects.First().Components.OfType<CameraComponent>().Single();
            Assert.Equal("Hero", cam.TargetTag);
            Assert.Equal(1.5f, cam.Zoom, 0.001f);
            Assert.True(cam.UseBounds);
            Assert.Equal(new Vector2(100, 50), cam.BoundsMax);
        }
    }
}
