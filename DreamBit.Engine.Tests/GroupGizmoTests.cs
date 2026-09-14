using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class GroupGizmoTests
    {
        [TestMethod]
        public void GroupBounds_EnglobaTodosOsObjetos()
        {
            var a = new GameObject("a");
            a.AddComponent(new SpriteRenderer { Size = new Vector2(100, 100) });
            a.Transform.Position = new Vector2(0, 0);

            var b = new GameObject("b");
            b.AddComponent(new SpriteRenderer { Size = new Vector2(100, 100) });
            b.Transform.Position = new Vector2(200, 0);

            var bounds = GizmoGeometry.GroupBounds(new[] { a, b });

            Assert.AreEqual(-50f, bounds.Min.X, 0.01f);
            Assert.AreEqual(250f, bounds.Max.X, 0.01f);
            Assert.AreEqual(-50f, bounds.Min.Y, 0.01f);
            Assert.AreEqual(50f, bounds.Max.Y, 0.01f);

            var center = GizmoGeometry.GroupCenter(bounds);
            Assert.AreEqual(100f, center.X, 0.01f);
            Assert.AreEqual(0f, center.Y, 0.01f);
        }

        [TestMethod]
        public void GroupCorners_SaoOsQuatroVertices()
        {
            var bounds = (Min: new Vector2(0, 0), Max: new Vector2(10, 20));
            var corners = GizmoGeometry.GroupCorners(bounds);

            Assert.AreEqual(4, corners.Length);
            Assert.AreEqual(new Vector2(0, 0), corners[0]);
            Assert.AreEqual(new Vector2(10, 0), corners[1]);
            Assert.AreEqual(new Vector2(10, 20), corners[2]);
            Assert.AreEqual(new Vector2(0, 20), corners[3]);
        }
    }
}
