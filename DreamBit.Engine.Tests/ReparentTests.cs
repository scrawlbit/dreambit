using DreamBit.Engine.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ReparentTests
    {
        [TestMethod]
        public void SetWorld_PreservaPoseAoReparentar()
        {
            var parent = new GameObject("P");
            parent.Transform.Position = new Vector2(100, 50);
            parent.Transform.Rotation = MathHelper.PiOver2; // 90°
            parent.Transform.Scale = new Vector2(2f, 2f);

            var child = new GameObject("C");
            child.Transform.Position = new Vector2(300, 10);

            var wp = child.Transform.WorldPosition;
            var wr = child.Transform.WorldRotation;
            var ws = child.Transform.WorldScale;

            // Reparenta e mantém a pose de mundo.
            parent.AddChild(child);
            child.Transform.SetWorld(wp, wr, ws);

            Assert.AreEqual(wp.X, child.Transform.WorldPosition.X, 0.01f);
            Assert.AreEqual(wp.Y, child.Transform.WorldPosition.Y, 0.01f);
            Assert.AreEqual(1f, child.Transform.WorldScale.X, 0.001f, "escala de mundo preservada");
            Assert.AreEqual(0f, NormalizedDelta(child.Transform.WorldRotation, wr), 0.001f, "rotação de mundo preservada");
        }

        [TestMethod]
        public void WorldScaleERotacao_CompoemComOPai()
        {
            var parent = new GameObject("P");
            parent.Transform.Rotation = 0.5f;
            parent.Transform.Scale = new Vector2(3f, 3f);
            var child = new GameObject("C");
            parent.AddChild(child);
            child.Transform.Rotation = 0.2f;
            child.Transform.Scale = new Vector2(2f, 2f);

            Assert.AreEqual(0.7f, child.Transform.WorldRotation, 0.001f);
            Assert.AreEqual(6f, child.Transform.WorldScale.X, 0.001f);
        }

        private static float NormalizedDelta(float a, float b)
        {
            float d = (a - b) % MathHelper.TwoPi;
            if (d > MathHelper.Pi) d -= MathHelper.TwoPi;
            if (d < -MathHelper.Pi) d += MathHelper.TwoPi;
            return d;
        }
    }
}
