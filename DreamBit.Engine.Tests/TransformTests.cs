using DreamBit.Engine.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class TransformTests
    {
        [TestMethod]
        public void WorldPosition_SemPai_IgualAPosicaoLocal()
        {
            var t = new Transform { Position = new Vector2(30, 40) };

            AssertClose(new Vector2(30, 40), t.WorldPosition);
        }

        [TestMethod]
        public void WorldPosition_ComPai_ComposicaoHierarquica()
        {
            var parent = new GameObject("pai");
            var child = new GameObject("filho");
            parent.AddChild(child);

            parent.Transform.Position = new Vector2(100, 0);
            child.Transform.Position = new Vector2(10, 5);

            AssertClose(new Vector2(110, 5), child.Transform.WorldPosition);
        }

        [TestMethod]
        public void WorldPosition_PaiRotacionado90Graus_RotacionaOFilho()
        {
            var parent = new GameObject("pai");
            var child = new GameObject("filho");
            parent.AddChild(child);

            parent.Transform.Rotation = MathHelper.PiOver2; // 90°
            child.Transform.Position = new Vector2(10, 0);

            // (10,0) girado 90° -> (0,10)
            AssertClose(new Vector2(0, 10), child.Transform.WorldPosition, 0.001f);
        }

        [TestMethod]
        public void Rotation_Normaliza_ParaIntervaloPositivo()
        {
            var t = new Transform { Rotation = -MathHelper.PiOver2 };

            Assert.IsTrue(t.Rotation > 0f);
            Assert.AreEqual(MathHelper.TwoPi - MathHelper.PiOver2, t.Rotation, 0.001f);
        }

        [TestMethod]
        public void Changed_Dispara_AoAlterarPosicao()
        {
            var t = new Transform();
            int changes = 0;
            t.Changed += () => changes++;

            t.Position = new Vector2(1, 1);

            Assert.IsTrue(changes >= 1);
        }

        private static void AssertClose(Vector2 expected, Vector2 actual, float tol = 0.01f)
        {
            Assert.AreEqual(expected.X, actual.X, tol, "X");
            Assert.AreEqual(expected.Y, actual.Y, tol, "Y");
        }
    }
}
