using System;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using System.Linq;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class CameraShakeTests
    {
        [Fact]
        public void Shake_DeslocaEDecai()
        {
            var cam = new CameraComponent { TargetTag = "", SmoothTime = 0f };
            var o = new GameObject("Cam"); o.Transform.Position = new Vector2(500, 500);
            o.AddComponent(cam);
            new Scene().Add(o);

            var camera = new Camera2D { Position = new Vector2(500, 500) };
            cam.Shake(0.5f, 20f);
            Assert.True(cam.IsShaking);
            cam.DriveCamera(camera, 0.016f, 800, 600);
            Assert.NotEqual(500f, camera.Position.X + camera.Position.Y - 500f);

            for (int i = 0; i < 40; i++) cam.DriveCamera(camera, 0.016f, 800, 600);
            Assert.False(cam.IsShaking, "o tremor termina");
        }

        [Fact]
        public void ShakeOnMessage_DisparaPeloBarramento()
        {
            var scene = new Scene();
            var o = new GameObject("Cam");
            var cam = new CameraComponent { ShakeOnMessage = "boom" };
            o.AddComponent(cam);
            scene.Add(o);
            scene.StartPlay();

            scene.Send("boom");
            scene.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)));
            Assert.True(cam.IsShaking, "a mensagem dispara o tremor");
        }

        [Fact]
        public void Serializacao_RoundTrip_Camera()
        {
            var scene = new Scene();
            var o = new GameObject("Cam");
            o.AddComponent(new CameraComponent { Priority = 5, ShakeOnMessage = "hit", ShakeMessageMagnitude = 18f });
            scene.Add(o);
            var e = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene)).Objects.First();
            var cam = e.Components.OfType<CameraComponent>().Single();
            Assert.Equal(5, cam.Priority);
            Assert.Equal("hit", cam.ShakeOnMessage);
            Assert.Equal(18f, cam.ShakeMessageMagnitude);
        }
    }
}
