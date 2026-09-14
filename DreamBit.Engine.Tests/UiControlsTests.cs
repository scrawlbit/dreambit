using System;
using System.Linq;
using DreamBit.Engine.Audio;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;
using GameInput = DreamBit.Engine.Input.Input;

namespace DreamBit.Engine.Tests
{
    public class UiControlsTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        [Fact]
        public void Slider_ArrastarDefineValorELigaAoBus()
        {
            AudioMixer.Reset();
            Screen.Set(800, 600);
            var scene = new Scene();
            var obj = new GameObject("Vol") { ScreenSpace = true };
            obj.Transform.Position = new Vector2(400, 300); // centro
            obj.AddComponent(new UiSlider { Width = 200, Height = 20, BusTarget = AudioMixer.Music });
            scene.Add(obj);
            var slider = obj.Components.OfType<UiSlider>().Single();

            scene.StartPlay();
            // trilho vai de x=300 a x=500; clicar/arrastar até x=350 => valor 0.25
            GameInput.SetPointer(new Vector2(400, 300), true); scene.Update(Frame); // pega no meio
            GameInput.SetPointer(new Vector2(350, 300), true); scene.Update(Frame); // arrasta p/ 0.25
            GameInput.SetPointer(new Vector2(350, 300), false); scene.Update(Frame);

            Assert.Equal(0.25f, slider.Value, 0.02f);
            Assert.Equal(0.25f, AudioMixer.GetVolume(AudioMixer.Music), 0.02f);
            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void Toggle_CliqueAlternaEDispara()
        {
            Screen.Set(800, 600);
            var scene = new Scene();
            var obj = new GameObject("Chk") { ScreenSpace = true };
            obj.Transform.Position = new Vector2(100, 100);
            obj.AddComponent(new UiToggle { Size = 30, SendOnChange = "mudou" });
            scene.Add(obj);
            var toggle = obj.Components.OfType<UiToggle>().Single();

            int mudou = 0;
            scene.MessageSent += m => { if (m.Name == "mudou") mudou++; };
            scene.StartPlay();

            Assert.False(toggle.IsOn);
            GameInput.SetPointer(new Vector2(100, 100), true); scene.Update(Frame);
            GameInput.SetPointer(new Vector2(100, 100), false); scene.Update(Frame);
            Assert.True(toggle.IsOn, "liga ao clicar");
            Assert.Equal(1, mudou);
            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void ProgressBar_FillProporcionalAoValor()
        {
            var obj = new GameObject("Vida") { ScreenSpace = true };
            obj.Transform.Position = new Vector2(0, 0);
            obj.AddComponent(new UiProgressBar { Width = 100, Height = 10, Value = 0.5f });
            var bar = obj.Components.OfType<UiProgressBar>().Single();

            Assert.Equal(50, bar.FillRect().Width);
            bar.Value = 0.25f;
            Assert.Equal(25, bar.FillRect().Width);
        }

        [Fact]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("UI") { ScreenSpace = true };
            obj.AddComponent(new UiSlider { Value = 0.3f, Width = 180, BusTarget = "SFX", SendOnChange = "vol" });
            var obj2 = new GameObject("UI2") { ScreenSpace = true };
            obj2.AddComponent(new UiToggle { IsOn = true, Size = 24, SendOnChange = "t" });
            obj2.AddComponent(new UiProgressBar { Value = 0.6f, Width = 90 });
            scene.Add(obj);
            scene.Add(obj2);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var s = loaded.Objects.First(o => o.Name == "UI").Components.OfType<UiSlider>().Single();
            Assert.Equal(0.3f, s.Value, 0.001f);
            Assert.Equal("SFX", s.BusTarget);
            var o2 = loaded.Objects.First(o => o.Name == "UI2");
            Assert.True(o2.Components.OfType<UiToggle>().Single().IsOn);
            Assert.Equal(0.6f, o2.Components.OfType<UiProgressBar>().Single().Value, 0.001f);
        }
    }
}
