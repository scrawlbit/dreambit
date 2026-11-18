using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class GroupCameraTests
    {
        private static (Scene, CameraComponent, GameObject, GameObject) Setup(float distance)
        {
            var scene = new Scene();
            var a = new GameObject("Ruber") { Tag = "Ruber" };
            var b = new GameObject("Aloine") { Tag = "Aloine" };
            a.Transform.Position = new Vector2(0, 0);
            b.Transform.Position = new Vector2(distance, 0);
            var camObj = new GameObject("Cam");
            var cam = camObj.AddComponent(new CameraComponent
            {
                TargetTag = "Ruber, Aloine", Zoom = 1f, MinZoom = 0.5f, GroupPadding = new Vector2(100, 100),
                SmoothTime = 0f, ZoomSmoothTime = 0f
            });
            scene.Add(a); scene.Add(b); scene.Add(camObj);
            return (scene, cam, a, b);
        }

        [Fact]
        public void Grupo_Proximo_MantemZoomNormal_ECentraliza()
        {
            var (_, cam, _, _) = Setup(400);
            var c2d = new Camera2D();
            cam.SnapCamera(c2d, 1280, 720);
            Assert.Equal(1f, c2d.Zoom, 3);
            Assert.Equal(new Vector2(200, 0), c2d.Position);
            Assert.False(cam.GroupOverflow);
        }

        [Fact]
        public void Grupo_Distante_DaZoomOut()
        {
            var (_, cam, _, _) = Setup(1800); // largura 1800 + 200 de margem = 2000 → 1280/2000 = 0.64
            var c2d = new Camera2D();
            cam.DriveCamera(c2d, 1f / 60, 1280, 720);
            Assert.Equal(0.64f, c2d.Zoom, 2);
            Assert.Equal(900f, c2d.Position.X, 1);
        }

        [Fact]
        public void Grupo_AlemDoLimite_SinalizaOverflow()
        {
            var (_, cam, _, _) = Setup(5000);
            var c2d = new Camera2D();
            cam.DriveCamera(c2d, 1f / 60, 1280, 720);
            Assert.Equal(0.5f, c2d.Zoom, 3);
            Assert.True(cam.GroupOverflow);
        }

        [Fact]
        public void BoundsOverride_TemPrioridade()
        {
            var (_, cam, _, _) = Setup(400);
            cam.BoundsOverride = (new Vector2(0, -1000), new Vector2(1280, 1000));
            var c2d = new Camera2D();
            cam.SnapCamera(c2d, 1280, 720);
            Assert.Equal(640f, c2d.Position.X, 1);
        }
    }
}
