using System;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ComponentEnabledTests
    {
        /// <summary>Componente de teste que conta os Updates recebidos.</summary>
        private sealed class Counter : SceneComponent
        {
            public int Ticks;
            public override string DisplayName => "Counter";
            protected override void Update(GameTime gameTime) => Ticks++;
        }

        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        [TestMethod]
        public void ComponenteDesligado_NaoRecebeUpdate()
        {
            var scene = new Scene();
            var o = new GameObject("O");
            var counter = new Counter();
            o.AddComponent(counter);
            scene.Add(o);
            scene.StartPlay();

            scene.Update(Frame);
            Assert.AreEqual(1, counter.Ticks);

            counter.Enabled = false;
            scene.Update(Frame);
            scene.Update(Frame);
            Assert.AreEqual(1, counter.Ticks, "desligado não recebe Update");

            counter.Enabled = true;
            scene.Update(Frame);
            Assert.AreEqual(2, counter.Ticks, "religado volta a receber Update");
        }

        [TestMethod]
        public void DesligarPlatformer_SuspendeGravidade()
        {
            // Caso "fase de voo": desligar o PlatformerController congela a queda.
            var scene = new Scene();
            var o = new GameObject("Voador");
            o.Transform.Position = new Vector2(0, 0);
            var plat = new PlatformerController { UseKeyboard = false, Gravity = 2000f };
            o.AddComponent(plat);
            scene.Add(o);
            scene.StartPlay();

            for (int i = 0; i < 5; i++) scene.Update(Frame);
            Assert.IsTrue(o.Transform.Position.Y > 0f, "cai com gravidade ligada");

            plat.Enabled = false;
            float frozen = o.Transform.Position.Y;
            for (int i = 0; i < 10; i++) scene.Update(Frame);
            Assert.AreEqual(frozen, o.Transform.Position.Y, 0.001f, "sem gravidade quando desligado (voo)");
        }
    }
}
