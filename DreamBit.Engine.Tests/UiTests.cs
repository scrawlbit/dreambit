using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class UiTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        [TestMethod]
        public void Anchor_ResolveCantosDaTela()
        {
            var a = new UiAnchor();

            a.Anchor = AnchorPoint.TopLeft;
            Assert.AreEqual(new Vector2(0, 0), a.Resolve(800, 600));

            a.Anchor = AnchorPoint.Center;
            Assert.AreEqual(new Vector2(400, 300), a.Resolve(800, 600));

            a.Anchor = AnchorPoint.BottomRight;
            a.OffsetX = -10; a.OffsetY = -10;
            Assert.AreEqual(new Vector2(790, 590), a.Resolve(800, 600));
        }

        [TestMethod]
        public void Anchor_PosicionaObjetoHudNoPlay()
        {
            Screen.Set(1000, 500);
            var scene = new Scene();
            var hud = new GameObject("Placar") { ScreenSpace = true };
            hud.AddComponent(new UiAnchor { Anchor = AnchorPoint.TopRight, OffsetX = -20, OffsetY = 12 });
            scene.Add(hud);

            scene.StartPlay();
            scene.Update(Frame);

            Assert.AreEqual(new Vector2(980, 12), hud.Transform.Position);
        }

        [TestMethod]
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
            Assert.IsNull(received);

            // pressiona e solta dentro do botão => dispara
            GameInput.SetPointer(new Vector2(400, 300), true);
            scene.Update(Frame);
            GameInput.SetPointer(new Vector2(400, 300), false);
            scene.Update(Frame);
            Assert.AreEqual("start", received);

            GameInput.ClearPointerOverride();
        }

        [TestMethod]
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

            Assert.IsFalse(fired);
            GameInput.ClearPointerOverride();
        }

        [TestMethod]
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

            Assert.AreEqual(AnchorPoint.BottomCenter, anchor.Anchor);
            Assert.AreEqual(5f, anchor.OffsetX);
            Assert.AreEqual(-7f, anchor.OffsetY);
            Assert.AreEqual(200f, button.Width);
            Assert.AreEqual("menu", button.SendOnClick);
            Assert.AreEqual(new Color(10, 20, 30), button.Normal);
        }
    }
}
