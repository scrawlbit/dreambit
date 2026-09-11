using DreamBit.Engine.Elements;
using DreamBit.Engine.Physics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class LedgePhysicsTests
    {
        private static Ledge Flat()
        {
            var l = new Ledge();
            l.SetPoints(new[] { new Vector2(0, 0), new Vector2(100, 0) });
            return l;
        }

        [TestMethod]
        public void SurfaceYAt_Plano_RetornaAAltura()
        {
            var l = Flat();
            Assert.AreEqual(0f, LedgePhysics.SurfaceYAt(l, 50), 0.01f);
            Assert.IsTrue(float.IsNaN(LedgePhysics.SurfaceYAt(l, -10)));
            Assert.IsTrue(float.IsNaN(LedgePhysics.SurfaceYAt(l, 200)));
        }

        [TestMethod]
        public void SurfaceYAt_Rampa_Interpola()
        {
            var l = new Ledge();
            l.SetPoints(new[] { new Vector2(0, 0), new Vector2(100, 100) });

            Assert.AreEqual(50f, LedgePhysics.SurfaceYAt(l, 50), 0.01f);
            Assert.AreEqual(25f, LedgePhysics.SurfaceYAt(l, 25), 0.01f);
        }

        [TestMethod]
        public void FindLanding_PousaAoCruzarDeCima()
        {
            var ledges = new[] { Flat() };

            // pés cruzando y=0 ao cair (de -5 para 5)
            Assert.AreEqual(0f, LedgePhysics.FindLanding(ledges, 50, -5, 5), 0.01f);
            // ainda acima (de -20 para -10): não pousa
            Assert.IsTrue(float.IsNaN(LedgePhysics.FindLanding(ledges, 50, -20, -10)));
            // já abaixo (de 10 para 20): não pousa
            Assert.IsTrue(float.IsNaN(LedgePhysics.FindLanding(ledges, 50, 10, 20)));
            // fora do span em X: não pousa
            Assert.IsTrue(float.IsNaN(LedgePhysics.FindLanding(ledges, 200, -5, 5)));
        }
    }
}
