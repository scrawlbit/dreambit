using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class AnimationEventTests
    {
        private static SpriteAnimator MakeAnimator(int frames, float fps, bool loop)
            => new() { FrameCount = frames, Fps = fps, Loop = loop };

        [Fact]
        public void Dispara_AoEntrarNoFrame()
        {
            var anim = MakeAnimator(4, 10f, loop: false); // 0.1s por frame
            anim.SetEvents(new[] { new AnimationFrameEvent(2, "hit") });

            var disparados = new List<string>();
            anim.AnimationEvent += disparados.Add;

            anim.Advance(0.05); // frame 0 ainda
            Assert.Empty(disparados);
            anim.Advance(0.10); // -> frame 1
            anim.Advance(0.10); // -> frame 2 (dispara)
            Assert.Single(disparados);
            Assert.Equal("hit", disparados[0]);
        }

        [Fact]
        public void NaoLoop_NaoDisparaOUltimoFrameDuasVezes()
        {
            var anim = MakeAnimator(3, 10f, loop: false);
            anim.SetEvents(new[] { new AnimationFrameEvent(2, "end") });

            int count = 0;
            anim.AnimationEvent += _ => count++;

            // Avança bastante além do fim; deve segurar no frame 2 sem re-disparar.
            for (int i = 0; i < 10; i++)
                anim.Advance(0.10);

            Assert.Equal(1, count);
        }

        [Fact]
        public void Loop_RedisparaACadaVolta()
        {
            var anim = MakeAnimator(2, 10f, loop: true);
            anim.SetEvents(new[] { new AnimationFrameEvent(0, "volta") });

            int count = 0;
            anim.AnimationEvent += _ => count++;

            // 2 frames a 10fps => 1 volta a cada 0.2s. Avança 0.4s em passos de frame.
            for (int i = 0; i < 4; i++)
                anim.Advance(0.10); // frames: 1,0(volta),1,0(volta)

            Assert.Equal(2, count);
        }

        [Fact]
        public void Serializacao_PreservaEventos()
        {
            var scene = new Scene();
            var obj = new GameObject("Herói");
            var anim = MakeAnimator(6, 8f, loop: true);
            anim.SetEvents(new[] { new AnimationFrameEvent(0, "passo"), new AnimationFrameEvent(3, "passo") });
            obj.AddComponent(anim);
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));

            var restored = loaded.Objects.First().Components.OfType<SpriteAnimator>().Single();
            var events = restored.Events.OrderBy(e => e.Frame).ToList();
            Assert.Equal(2, events.Count);
            Assert.Equal(0, events[0].Frame);
            Assert.Equal("passo", events[0].Name);
            Assert.Equal(3, events[1].Frame);
        }
    }
}
