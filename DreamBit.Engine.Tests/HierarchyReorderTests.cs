using System.Linq;
using DreamBit.Engine.Elements;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class HierarchyReorderTests
    {
        [Fact]
        public void MoveObject_ReordenaRaizes()
        {
            var scene = new Scene();
            var a = scene.Add(new GameObject("A"));
            var b = scene.Add(new GameObject("B"));
            var c = scene.Add(new GameObject("C"));

            scene.MoveObject(a, 2); // A vai para o fim

            Assert.Equal(new[] { "B", "C", "A" }, scene.Objects.Select(o => o.Name).ToArray());
            Assert.Equal(2, scene.IndexOf(a));
        }

        [Fact]
        public void MoveChild_ReordenaFilhos()
        {
            var scene = new Scene();
            var group = scene.Add(new GameObject("G"));
            var x = new GameObject("X"); group.AddChild(x);
            var y = new GameObject("Y"); group.AddChild(y);
            var z = new GameObject("Z"); group.AddChild(z);

            group.MoveChild(z, 0); // Z vai para o começo

            Assert.Equal(new[] { "Z", "X", "Y" }, group.Children.Select(o => o.Name).ToArray());
            Assert.Equal(0, group.IndexOfChild(z));
        }

        [Fact]
        public void InsertChild_ReparentaNaPosicao()
        {
            var scene = new Scene();
            var g1 = scene.Add(new GameObject("G1"));
            var g2 = scene.Add(new GameObject("G2"));
            var item = new GameObject("Item"); g1.AddChild(item);

            g2.InsertChild(item, 0); // move de g1 para g2

            Assert.Empty(g1.Children);
            Assert.Single(g2.Children);
            Assert.Same(g2, item.Parent);
        }
    }
}
