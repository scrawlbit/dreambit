using System;
using System.Collections.Generic;
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
    public class UiNavigationTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        private static UiButton ButtonAt(Scene scene, float x, float y, string send)
        {
            var o = new GameObject($"B{x}_{y}") { ScreenSpace = true };
            o.Transform.Position = new Vector2(x, y);
            var b = new UiButton { Width = 100, Height = 40, SendOnClick = send };
            o.AddComponent(b);
            scene.Add(o);
            return b;
        }

        [Fact]
        public void PickNext_VaiParaODeBaixo()
        {
            var scene = new Scene();
            var top = ButtonAt(scene, 100, 100, "a");
            var mid = ButtonAt(scene, 100, 160, "b");
            var bottom = ButtonAt(scene, 100, 220, "c");

            var list = new List<IUiFocusable> { top, mid, bottom };
            var next = UiNavigator.PickNext(list, top, 0, 1);
            Assert.Same(mid, next);
            Assert.Null(UiNavigator.PickNext(list, bottom, 0, 1));
        }

        [Fact]
        public void Slider_NudgeAjustaValorEBus()
        {
            AudioMixer.Reset();
            AudioMixer.SetVolume(AudioMixer.Music, 0.5f); // o slider sincroniza com o bus no play
            var scene = new Scene();
            var o = new GameObject("Vol") { ScreenSpace = true };
            o.AddComponent(new UiSlider { BusTarget = AudioMixer.Music });
            scene.Add(o);
            var slider = o.Components.OfType<UiSlider>().Single();
            scene.StartPlay();
            Assert.Equal(0.5f, slider.Value, 0.001f);

            slider.Nudge(1);
            Assert.Equal(0.55f, slider.Value, 0.001f);
            Assert.Equal(0.55f, AudioMixer.GetVolume(AudioMixer.Music), 0.001f);
            slider.Nudge(-1);
            Assert.Equal(0.5f, slider.Value, 0.001f);
        }

        [Fact]
        public void Navigator_AutoFocaOPrimeiro()
        {
            Screen.Set(800, 600);
            UiFocus.Clear();
            var scene = new Scene();
            var first = ButtonAt(scene, 100, 100, "a");
            ButtonAt(scene, 100, 160, "b");
            var navObj = new GameObject("Nav") { ScreenSpace = true };
            navObj.AddComponent(new UiNavigator());
            scene.Add(navObj);

            scene.StartPlay();
            GameInput.SetPointer(new Vector2(-1, -1), false);
            scene.Update(Frame);

            Assert.True(UiFocus.Has(first), "foca o primeiro controle automaticamente");
            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void Activate_DisparaOControleFocado()
        {
            var scene = new Scene();
            var b = ButtonAt(scene, 100, 100, "go");
            string? got = null;
            scene.MessageSent += m => got = m.Name;
            scene.StartPlay();

            UiFocus.Set(b);
            b.Activate();
            GameInput.SetPointer(new Vector2(-1, -1), false);
            scene.Update(Frame); // despacha a mensagem enfileirada
            Assert.Equal("go", got);
            GameInput.ClearPointerOverride();
        }

        [Fact]
        public void Navigator_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var o = new GameObject("Nav") { ScreenSpace = true };
            o.AddComponent(new UiNavigator { AutoFocusFirst = false });
            scene.Add(o);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            Assert.False(loaded.Objects.First().Components.OfType<UiNavigator>().Single().AutoFocusFirst);
        }
    }
}
