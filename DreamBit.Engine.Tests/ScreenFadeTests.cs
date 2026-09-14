using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class ScreenFadeTests
    {
        private static GameTime Frame(double s) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        [Fact]
        public void FadeOut_EscureceAoLongoDoTempo()
        {
            var scene = new Scene();
            var o = new GameObject("Fade");
            var fade = new ScreenFade();
            o.AddComponent(fade);
            scene.Add(o);
            scene.StartPlay();

            fade.FadeOut(0.5f);
            for (int i = 0; i < 8; i++) scene.Update(Frame(0.033));
            Assert.True(fade.Alpha > 0.3f && fade.Alpha < 1f, $"escurecendo (alpha={fade.Alpha:0.00})");
            for (int i = 0; i < 10; i++) scene.Update(Frame(0.033));
            Assert.Equal(1f, fade.Alpha, 0.01f);
        }

        [Fact]
        public void FlashPorMensagem_SobeEDecai()
        {
            var scene = new Scene();
            var o = new GameObject("Fade");
            o.AddComponent(new ScreenFade { FlashOnMessage = "hit", FlashDuration = 0.3f, Color = Color.Red });
            scene.Add(o);
            var fade = o.Components.OfType<ScreenFade>().Single();
            scene.StartPlay();

            scene.Send("hit");
            scene.Update(Frame(0.016)); // despacha a mensagem -> Flash
            Assert.True(fade.Alpha > 0.5f, "flash sobe");
            for (int i = 0; i < 20; i++) scene.Update(Frame(0.033));
            Assert.Equal(0f, fade.Alpha, 0.02f);
        }

        [Fact]
        public void Serializacao_RoundTrip_ScreenFade()
        {
            var scene = new Scene();
            var o = new GameObject("F");
            o.AddComponent(new ScreenFade { Color = new Color(20, 30, 40), Alpha = 0.5f, FlashOnMessage = "boom", FlashDuration = 0.4f });
            scene.Add(o);
            var f = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene)).Objects.First()
                .Components.OfType<ScreenFade>().Single();
            Assert.Equal(new Color(20, 30, 40), f.Color);
            Assert.Equal("boom", f.FlashOnMessage);
            Assert.Equal(0.4f, f.FlashDuration);
        }
    }
}
