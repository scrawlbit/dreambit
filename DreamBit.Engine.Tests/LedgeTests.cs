using System.IO;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class LedgeTests
    {
        [TestMethod]
        public void Segments_SaoOsParesConsecutivos()
        {
            var ledge = new Ledge();
            ledge.AddPoint(new Vector2(0, 0));
            ledge.AddPoint(new Vector2(10, 0));
            ledge.AddPoint(new Vector2(20, 0));

            Assert.AreEqual(2, ledge.Segments().Count());
        }

        [TestMethod]
        public void DistanceTo_PontoSobreOSegmento_EhZero()
        {
            var ledge = new Ledge();
            ledge.AddPoint(new Vector2(0, 0));
            ledge.AddPoint(new Vector2(100, 0));

            Assert.AreEqual(0f, ledge.DistanceTo(new Vector2(50, 0)), 0.01f);
            Assert.AreEqual(10f, ledge.DistanceTo(new Vector2(50, 10)), 0.01f);
        }

        [TestMethod]
        public void Serializacao_PreservaLedges()
        {
            var scene = new Scene { Name = "Mapa" };
            var ledge = new Ledge { Name = "Chao", OneWay = false };
            ledge.SetPoints(new[] { new Vector2(0, 0), new Vector2(64, 8), new Vector2(128, 0) });
            scene.AddLedge(ledge);

            var path = Path.Combine(Path.GetTempPath(), "dreambit_ledge_test.dbscene");
            try
            {
                SceneSerializer.Save(scene, path);
                var loaded = SceneSerializer.Load(path);

                Assert.AreEqual(1, loaded.Ledges.Count);
                var loadedLedge = loaded.Ledges[0];
                Assert.AreEqual("Chao", loadedLedge.Name);
                Assert.IsFalse(loadedLedge.OneWay);
                Assert.AreEqual(3, loadedLedge.Points.Count);
                Assert.AreEqual(64f, loadedLedge.Points[1].X, 0.01f);
                Assert.AreEqual(8f, loadedLedge.Points[1].Y, 0.01f);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
