using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using DreamBit.Engine.Timing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class TimingTests
    {
        private static GameTime Frame(float dt) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(dt));

        [TestMethod]
        public void Timer_DisparaMensagemAoEsgotar()
        {
            var scene = new Scene();
            var obj = new GameObject("Bomba");
            obj.AddComponent(new TimerComponent { Duration = 1f, Repeat = false, SendOnElapsed = "boom" });
            scene.Add(obj);

            int count = 0;
            scene.MessageSent += m => { if (m.Name == "boom") count++; };
            scene.StartPlay();

            for (int i = 0; i < 30; i++) // 30 * 0.05 = 1.5s
                scene.Update(Frame(0.05f));

            Assert.AreEqual(1, count, "one-shot dispara uma única vez");
        }

        [TestMethod]
        public void Timer_RepeteQuandoConfigurado()
        {
            var scene = new Scene();
            var obj = new GameObject("Spawner");
            obj.AddComponent(new TimerComponent { Duration = 0.5f, Repeat = true, SendOnElapsed = "spawn" });
            scene.Add(obj);

            int count = 0;
            scene.MessageSent += m => { if (m.Name == "spawn") count++; };
            scene.StartPlay();

            for (int i = 0; i < 40; i++) // 2.0s => ~4 disparos (0.5s cada)
                scene.Update(Frame(0.05f));

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void Timer_StartOnReiniciaPelaMensagem()
        {
            var scene = new Scene();
            var obj = new GameObject("PortaTemporizada");
            obj.AddComponent(new TimerComponent { Duration = 1f, AutoStart = false, StartOn = "abrir", SendOnElapsed = "fechar" });
            scene.Add(obj);

            int fechou = 0;
            scene.MessageSent += m => { if (m.Name == "fechar") fechou++; };
            scene.StartPlay();

            for (int i = 0; i < 20; i++) scene.Update(Frame(0.05f)); // não iniciou => nada
            Assert.AreEqual(0, fechou);

            scene.Send("abrir");
            scene.Update(Frame(0f)); // despacha a mensagem => timer inicia
            for (int i = 0; i < 30; i++) scene.Update(Frame(0.05f));
            Assert.AreEqual(1, fechou);
        }

        [TestMethod]
        public void Scheduler_AfterEEvery()
        {
            Scheduler.Clear();
            int after = 0, every = 0;
            Scheduler.After(1f, () => after++);
            Scheduler.Every(0.5f, () => every++);

            for (int i = 0; i < 40; i++) // 2.0s
                Scheduler.Tick(0.05f);

            Assert.AreEqual(1, after);
            Assert.AreEqual(4, every);
            Scheduler.Clear();
        }

        [TestMethod]
        public void Scheduler_CancelImpedeDisparo()
        {
            Scheduler.Clear();
            bool fired = false;
            var handle = Scheduler.After(0.5f, () => fired = true);
            Scheduler.Tick(0.1f);
            Scheduler.Cancel(handle);
            for (int i = 0; i < 10; i++) Scheduler.Tick(0.1f);
            Assert.IsFalse(fired);
        }

        [TestMethod]
        public void StartPlay_LimpaScheduler()
        {
            Scheduler.Clear();
            Scheduler.After(1f, () => { });
            Assert.AreEqual(1, Scheduler.Count);

            new Scene().StartPlay();
            Assert.AreEqual(0, Scheduler.Count, "StartPlay limpa agendamentos antigos");
        }

        [TestMethod]
        public void Timer_Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("T");
            obj.AddComponent(new TimerComponent { Duration = 2.5f, Repeat = true, AutoStart = false, SendOnElapsed = "tick", StartOn = "go" });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var t = loaded.Objects.First().Components.OfType<TimerComponent>().Single();
            Assert.AreEqual(2.5f, t.Duration, 0.001f);
            Assert.IsTrue(t.Repeat);
            Assert.IsFalse(t.AutoStart);
            Assert.AreEqual("tick", t.SendOnElapsed);
            Assert.AreEqual("go", t.StartOn);
        }
    }
}
