using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class UiLayoutTests
    {
        [TestMethod]
        public void EmpilhaFilhosNaVertical()
        {
            var menu = new GameObject("Menu");
            menu.AddComponent(new UiLayout { Direction = LayoutDirection.Vertical, Spacing = 10f });

            for (int i = 0; i < 3; i++)
            {
                var b = new GameObject("B" + i);
                b.AddComponent(new UiButton { Width = 100, Height = 40 });
                menu.AddChild(b);
            }

            menu.Components.OfType<UiLayout>().Single().Arrange();

            var ys = menu.Children.Select(c => c.Transform.Position.Y).ToArray();
            // 3 itens de 40 + 2 espaços de 10 = 140; centralizado => -70..70; centros: -50, 0, 50
            Assert.AreEqual(-50f, ys[0], 0.01f);
            Assert.AreEqual(0f, ys[1], 0.01f);
            Assert.AreEqual(50f, ys[2], 0.01f);
            Assert.IsTrue(menu.Children.All(c => c.Transform.Position.X == 0f));
        }

        [TestMethod]
        public void HorizontalUsaLargura()
        {
            var bar = new GameObject("Bar");
            bar.AddComponent(new UiLayout { Direction = LayoutDirection.Horizontal, Spacing = 0f });
            var a = new GameObject("a"); a.AddComponent(new UiButton { Width = 60, Height = 20 });
            var b = new GameObject("b"); b.AddComponent(new UiButton { Width = 60, Height = 20 });
            bar.AddChild(a); bar.AddChild(b);

            bar.Components.OfType<UiLayout>().Single().Arrange();

            Assert.AreEqual(-30f, a.Transform.Position.X, 0.01f);
            Assert.AreEqual(30f, b.Transform.Position.X, 0.01f);
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Menu");
            obj.AddComponent(new UiLayout { Direction = LayoutDirection.Horizontal, Spacing = 7f });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var l = loaded.Objects.First().Components.OfType<UiLayout>().Single();
            Assert.AreEqual(LayoutDirection.Horizontal, l.Direction);
            Assert.AreEqual(7f, l.Spacing, 0.001f);
        }
    }
}
