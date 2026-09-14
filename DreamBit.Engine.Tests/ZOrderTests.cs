using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class ZOrderTests
    {
        [Fact]
        public void OrdenaPorSortOrder_EstavelNosEmpates()
        {
            var scene = new Scene();
            var a = new GameObject("A") { SortOrder = 10 };
            var b = new GameObject("B") { SortOrder = -5 };
            var c = new GameObject("C") { SortOrder = 0 };
            var d = new GameObject("D") { SortOrder = 0 };
            scene.Add(a); scene.Add(b); scene.Add(c); scene.Add(d);

            var order = scene.VisibleInDrawOrder().Select(o => o.Name).ToArray();
            CollectionAssert.AreEqual(new[] { "B", "C", "D", "A" }, order);
        }

        [Fact]
        public void SubarvoreInvisivel_NaoEntra()
        {
            var scene = new Scene();
            var parent = new GameObject("Pai") { IsVisible = false };
            var child = new GameObject("Filho");
            parent.AddChild(child);
            scene.Add(parent);
            scene.Add(new GameObject("Outro"));

            var names = scene.VisibleInDrawOrder().Select(o => o.Name).ToArray();
            CollectionAssert.AreEqual(new[] { "Outro" }, names);
        }

        [Fact]
        public void Serializacao_PreservaSortOrder()
        {
            var scene = new Scene();
            scene.Add(new GameObject("X") { SortOrder = 42 });
            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            Assert.Equal(42, loaded.Objects.First().SortOrder);
        }
    }
}
