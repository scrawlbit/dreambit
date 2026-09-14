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
    public class UiTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        [Fact]
        public void Anchor_ResolveCantosDaTela()
        {
            var a = new UiAnchor();

            a.Anchor = AnchorPoint.TopLeft;
            Assert.Equal(new Vector2(0, 0), a.Resolve(800, 600));

            a.Anchor = AnchorPoint.Center;
            Assert.Equal(new Vector2(400, 300), a.Resolve(800, 600));

            a.Anchor = AnchorPoint.BottomRight;
            a.OffsetX = -10; a.OffsetY = -10;
            Assert.Equal(new Vector2(790, 590), a.Resolve(800, 600));
        }

        [Fact]
        public void Anchor_PosicionaObjetoHudNoPlay()
        {
            Screen.Set(1000, 500);
            var scene = new Scene();
            var hud = new GameObject("Placar") { ScreenSpace = true };
            hud.AddComponent(new UiAnchor { Anchor = AnchorPoint.TopRight, OffsetX = -20, OffsetY = 12 });
            scene.Add(hud);

            scene.StartPlay();
            scene.Update(Frame);

            Assert.Equal(new Vector2(980, 12), hud.Transform.Position);
        }

        [Fact]
        public void Button_CliqueDentroDisparaMensagem()
        {
            Screen.Set(800, 600);
            var scene = new Scene();
            var btn = new GameObject("Jogar") { ScreenSpace = true };
            btn.Transform.Position = new Vector2(400, 300);
            btn.AddComponent(new UiButton { Width = 160, Height = 48, SendOnClick = "start" });
            scene.Add(btn);

            string? received = null;
            scene.MessageSent += m => received = m.Name;
            scene.StartPlay();

            // ponteiro fora: sem clique
            GameInput.SetPointer(new Vector2(10, 10), true);
            scene.Update(Frame);
            GameInput.SetPointer(new Vector2(10, 10), false);
            scene.Update(Frame);
            Assert.Null(received);

            // pressiona e solta dentro do botão => dispara
            GameInput.SetPointer(new Vector2(400, 300), true);
            scene.Update(Frame);
            GameInput.SetPointer(new Vector2(400, 300), false);
            scene.Update(Frame);
            Assert.Equal("start", received);

            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void Button_SoltarForaNaoDispara()
        {
            Screen.Set(800, 600);
            var scene = new Scene();
            var btn = new GameObject("B") { ScreenSpace = true };
            btn.Transform.Position = new Vector2(400, 300);
            btn.AddComponent(new UiButton { Width = 100, Height = 40, SendOnClick = "x" });
            scene.Add(btn);

            bool fired = false;
            scene.MessageSent += _ => fired = true;
            scene.StartPlay();

            GameInput.SetPointer(new Vector2(400, 300), true); // pressiona dentro
            scene.Update(Frame);
            GameInput.SetPointer(new Vector2(50, 50), false);  // solta fora
            scene.Update(Frame);

            Assert.False(fired);
            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("HUD") { ScreenSpace = true };
            obj.AddComponent(new UiAnchor { Anchor = AnchorPoint.BottomCenter, OffsetX = 5, OffsetY = -7 });
            obj.AddComponent(new UiButton { Width = 200, Height = 60, SendOnClick = "menu", Normal = new Color(10, 20, 30) });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lo = loaded.Objects.First();
            var anchor = lo.Components.OfType<UiAnchor>().Single();
            var button = lo.Components.OfType<UiButton>().Single();

            Assert.Equal(AnchorPoint.BottomCenter, anchor.Anchor);
            Assert.Equal(5f, anchor.OffsetX);
            Assert.Equal(-7f, anchor.OffsetY);
            Assert.Equal(200f, button.Width);
            Assert.Equal("menu", button.SendOnClick);
            Assert.Equal(new Color(10, 20, 30), button.Normal);
        }
    }
}
