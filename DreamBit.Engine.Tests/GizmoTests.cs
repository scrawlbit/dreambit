using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class GizmoTests
    {
        [TestMethod]
        public void RotationTowards_AlvoAcima_RotacaoZero()
        {
            var obj = new GameObject();
            // "acima" na tela é -Y
            float r = GizmoGeometry.RotationTowards(obj, new Vector2(0, -100));
            Assert.AreEqual(0f, r, 0.001f);
        }

        [TestMethod]
        public void RotationTowards_AlvoADireita_MeiaVoltaPositiva()
        {
            var obj = new GameObject();
            float r = GizmoGeometry.RotationTowards(obj, new Vector2(100, 0));
            Assert.AreEqual(MathHelper.PiOver2, r, 0.001f);
        }

        [TestMethod]
        public void HandleFicaAcimaDoCentro_QuandoRotacaoZero()
        {
            var obj = new GameObject();
            obj.Transform.Position = new Vector2(10, 10);

            var handle = GizmoGeometry.RotationHandleWorld(obj, 1f);

            Assert.AreEqual(10f, handle.X, 0.01f);
            Assert.IsTrue(handle.Y < 10f, "handle deve ficar acima (Y menor) do centro");
        }
    }
}
