using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class PropertyAnimatorTests
    {
        private static GameTime Frame(float dt) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(dt));

        [TestMethod]
        public void Track_InterpolaEntreKeyframes()
        {
            var track = new PropertyTrack { Channel = AnimChannel.PositionY, Easing = Scrawlbit.EasingMode.Linear };
            track.Keys.Add(new AnimKey(0f, 0f));
            track.Keys.Add(new AnimKey(1f, 100f));

            Assert.AreEqual(0f, track.Sample(0f), 0.01f);
            Assert.AreEqual(50f, track.Sample(0.5f), 0.01f);
            Assert.AreEqual(100f, track.Sample(1f), 0.01f);
            Assert.AreEqual(0f, track.Sample(-5f), 0.01f, "antes do primeiro = primeiro");
            Assert.AreEqual(100f, track.Sample(9f), 0.01f, "depois do último = último");
        }

        [TestMethod]
        public void AnimaPosicaoNoPlay()
        {
            var scene = new Scene();
            var obj = new GameObject("O");
            var pa = new PropertyAnimator { Duration = 1f, Loop = TweenLoop.Once, PlayOnStart = true };
            var track = new PropertyTrack { Channel = AnimChannel.PositionX, Easing = Scrawlbit.EasingMode.Linear };
            track.Keys.Add(new AnimKey(0f, 0f));
            track.Keys.Add(new AnimKey(1f, 200f));
            pa.Tracks.Add(track);
            obj.AddComponent(pa);
            scene.Add(obj);

            scene.StartPlay();
            for (int i = 0; i < 25; i++) scene.Update(Frame(0.02f)); // 0.5s
            Assert.AreEqual(100f, obj.Transform.Position.X, 6f, "metade do caminho");

            for (int i = 0; i < 40; i++) scene.Update(Frame(0.02f)); // termina
            Assert.AreEqual(200f, obj.Transform.Position.X, 1f);
        }

        [TestMethod]
        public void AnimaCorDoSprite()
        {
            var scene = new Scene();
            var obj = new GameObject("O");
            obj.AddComponent(new SpriteRenderer { Color = new Color(0, 0, 0, 255) });
            var pa = new PropertyAnimator { Duration = 1f, Loop = TweenLoop.Once };
            var track = new PropertyTrack { Channel = AnimChannel.SpriteR, Easing = Scrawlbit.EasingMode.Linear };
            track.Keys.Add(new AnimKey(0f, 0f));
            track.Keys.Add(new AnimKey(1f, 255f));
            pa.Tracks.Add(track);
            obj.AddComponent(pa);
            scene.Add(obj);

            scene.StartPlay();
            for (int i = 0; i < 50; i++) scene.Update(Frame(0.02f));
            Assert.IsTrue(obj.Components.OfType<SpriteRenderer>().Single().Color.R > 200, "R subiu");
        }

        [TestMethod]
        public void EffectiveDuration_UsaMaiorKeyframeQuandoZero()
        {
            var pa = new PropertyAnimator { Duration = 0f };
            var track = new PropertyTrack { Channel = AnimChannel.Rotation };
            track.Keys.Add(new AnimKey(0f, 0f));
            track.Keys.Add(new AnimKey(2.5f, 90f));
            pa.Tracks.Add(track);
            Assert.AreEqual(2.5f, pa.EffectiveDuration, 0.001f);
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("O");
            var pa = new PropertyAnimator { Duration = 3f, Loop = TweenLoop.PingPong, PlayOnStart = false };
            var t1 = new PropertyTrack { Channel = AnimChannel.PositionY, Easing = Scrawlbit.EasingMode.In };
            t1.Keys.Add(new AnimKey(0f, 10f)); t1.Keys.Add(new AnimKey(1.5f, 60f));
            pa.Tracks.Add(t1);
            obj.AddComponent(pa);
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var a = loaded.Objects.First().Components.OfType<PropertyAnimator>().Single();
            Assert.AreEqual(3f, a.Duration);
            Assert.AreEqual(TweenLoop.PingPong, a.Loop);
            Assert.IsFalse(a.PlayOnStart);
            Assert.AreEqual(1, a.Tracks.Count);
            Assert.AreEqual(AnimChannel.PositionY, a.Tracks[0].Channel);
            Assert.AreEqual(2, a.Tracks[0].Keys.Count);
            Assert.AreEqual(60f, a.Tracks[0].Keys[1].Value, 0.001f);
        }
    }
}
