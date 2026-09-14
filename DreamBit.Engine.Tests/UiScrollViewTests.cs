using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    public class UiScrollViewTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        private static (Scene, UiScrollView) BuildList(int count)
        {
            Screen.Set(800, 600);
            var scene = new Scene();
            var panel = new GameObject("Lista") { ScreenSpace = true };
            panel.Transform.Position = new Vector2(400, 300);
            // viewport de 100 de altura; itens de 40 => cabem ~2 por vez
            panel.AddComponent(new UiScrollView { Width = 200, Height = 100, Spacing = 0, ScrollSpeed = 40 });
            for (int i = 0; i < count; i++)
            {
                var item = new GameObject("Item" + i);
                item.AddComponent(new UiButton { Width = 180, Height = 40 });
                panel.AddChild(item);
            }
            scene.Add(panel);
            return (scene, panel.Components.OfType<UiScrollView>().Single());
        }

        [Fact]
        public void CullaItensForaDaViewport()
        {
            var (scene, _) = BuildList(6);
            var panel = scene.VisibleInDrawOrder().First(o => o.Name == "Lista");
            scene.StartPlay();
            GameInput.SetPointer(new Vector2(-1, -1), false);
            scene.Update(Frame);

            int visiveis = panel.Children.Count(c => c.IsVisible);
            Assert.True(visiveis >= 1 && visiveis < 6, $"só parte da lista visível (visíveis={visiveis})");
        }

        [Fact]
        public void RolarMostraItensDeBaixo()
        {
            var (scene, scroll) = BuildList(6);
            var panel = scene.VisibleInDrawOrder().First(o => o.Name == "Lista");
            scene.StartPlay();
            GameInput.SetPointer(new Vector2(-1, -1), false);
            scene.Update(Frame);

            bool ultimoAntes = panel.Children.Last().IsVisible;

            scroll.Scroll = 1000f; // além do fim; será clampado
            scene.Update(Frame);

            bool ultimoDepois = panel.Children.Last().IsVisible;
            Assert.False(ultimoAntes, "último não aparece no topo");
            Assert.True(ultimoDepois, "aparece após rolar até o fim");
            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var o = new GameObject("S") { ScreenSpace = true };
            o.AddComponent(new UiScrollView { Width = 300, Height = 200, Spacing = 12, ScrollSpeed = 30 });
            scene.Add(o);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var s = loaded.Objects.First().Components.OfType<UiScrollView>().Single();
            Assert.Equal(300f, s.Width);
            Assert.Equal(200f, s.Height);
            Assert.Equal(12f, s.Spacing);
            Assert.Equal(30f, s.ScrollSpeed);
        }
    }
}
