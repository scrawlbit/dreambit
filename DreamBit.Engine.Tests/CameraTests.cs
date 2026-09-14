using DreamBit.Engine.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class CameraTests
    {
        [TestMethod]
        public void CentroDoViewport_MapeiaParaPosicaoDaCamera()
        {
            var camera = new Camera2D { Position = new Vector2(50, 20) };

            var world = camera.ScreenToWorld(new Vector2(400, 300), 800, 600);

            Assert.AreEqual(50f, world.X, 0.01f);
            Assert.AreEqual(20f, world.Y, 0.01f);
        }

        [TestMethod]
        public void ZoomAt_MantemOPontoSobOCursor()
        {
            var camera = new Camera2D();
            var screen = new Vector2(200, 150);

            var before = camera.ScreenToWorld(screen, 800, 600);
            camera.ZoomAt(screen, 2f, 800, 600);
            var after = camera.ScreenToWorld(screen, 800, 600);

            Assert.AreEqual(before.X, after.X, 0.01f);
            Assert.AreEqual(before.Y, after.Y, 0.01f);
        }

        [TestMethod]
        public void Zoom_LimitadoAoIntervalo()
        {
            var camera = new Camera2D { Zoom = 999f };
            Assert.AreEqual(10f, camera.Zoom, 0.001f);

            camera.Zoom = 0.0001f;
            Assert.AreEqual(0.1f, camera.Zoom, 0.001f);
        }
    }
}
