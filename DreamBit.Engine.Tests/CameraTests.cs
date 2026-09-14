using DreamBit.Engine.Rendering;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class CameraTests
    {
        [Fact]
        public void CentroDoViewport_MapeiaParaPosicaoDaCamera()
        {
            var camera = new Camera2D { Position = new Vector2(50, 20) };

            var world = camera.ScreenToWorld(new Vector2(400, 300), 800, 600);

            Assert.Equal(50f, world.X, 0.01f);
            Assert.Equal(20f, world.Y, 0.01f);
        }

        [Fact]
        public void ZoomAt_MantemOPontoSobOCursor()
        {
            var camera = new Camera2D();
            var screen = new Vector2(200, 150);

            var before = camera.ScreenToWorld(screen, 800, 600);
            camera.ZoomAt(screen, 2f, 800, 600);
            var after = camera.ScreenToWorld(screen, 800, 600);

            Assert.Equal(before.X, after.X, 0.01f);
            Assert.Equal(before.Y, after.Y, 0.01f);
        }

        [Fact]
        public void Zoom_LimitadoAoIntervalo()
        {
            var camera = new Camera2D { Zoom = 999f };
            Assert.Equal(10f, camera.Zoom, 0.001f);

            camera.Zoom = 0.0001f;
            Assert.Equal(0.1f, camera.Zoom, 0.001f);
        }
    }
}
