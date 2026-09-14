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
    public class TweenTests
    {
        private static GameTime Step(double s) => new(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        [TestMethod]
        public void AnimaPositionY_Linear_AteOFim()
        {
            var scene = new Scene();
            var obj = new GameObject("O");
            obj.AddComponent(new TweenComponent
            {
                Channel = TweenChannel.PositionY, From = 0, To = 100, Duration = 1f,
                Easing = Scrawlbit.EasingMode.Linear, Loop = TweenLoop.Once, PlayOnStart = true
            });
            scene.Add(obj);

            scene.StartPlay();
            scene.Update(Step(0.5)); // metade
            Assert.AreEqual(50f, obj.Transform.Position.Y, 0.5f);

            scene.Update(Step(1.0)); // passa do fim -> segura em To
            Assert.AreEqual(100f, obj.Transform.Position.Y, 0.5f);
        }

        [TestMethod]
        public void PingPong_VoltaAoInicio()
        {
            var scene = new Scene();
            var obj = new GameObject("O");
            obj.AddComponent(new TweenComponent
            {
                Channel = TweenChannel.PositionX, From = 0, To = 100, Duration = 1f,
                Easing = Scrawlbit.EasingMode.Linear, Loop = TweenLoop.PingPong, PlayOnStart = true
            });
            scene.Add(obj);
            scene.StartPlay();

            scene.Update(Step(1.0)); // ida: no pico (~100)
            Assert.AreEqual(100f, obj.Transform.Position.X, 1f);
            scene.Update(Step(1.0)); // volta: ~0
            Assert.AreEqual(0f, obj.Transform.Position.X, 1f);
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("O");
            obj.AddComponent(new TweenComponent { Channel = TweenChannel.Rotation, From = 0, To = 90, Duration = 2f, Loop = TweenLoop.Loop });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var tw = loaded.Objects.First().Components.OfType<TweenComponent>().Single();
            Assert.AreEqual(TweenChannel.Rotation, tw.Channel);
            Assert.AreEqual(90f, tw.To, 0.001f);
            Assert.AreEqual(TweenLoop.Loop, tw.Loop);
        }
    }
}
