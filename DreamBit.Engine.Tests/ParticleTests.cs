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
    public class ParticleTests
    {
        [TestMethod]
        public void Burst_EmiteVariasDeUmaVez()
        {
            var scene = new Scene();
            var o = new GameObject("Fx");
            var pe = new ParticleEmitter { EmitRate = 0f, BurstCount = 20, EmitOnStart = true, Lifetime = 2f };
            o.AddComponent(pe);
            scene.Add(o);
            scene.StartPlay();
            Assert.AreEqual(20, pe.ActiveParticles, "burst emite tudo no início");

            pe.Burst(5);
            Assert.AreEqual(25, pe.ActiveParticles);
        }

        [TestMethod]
        public void Serializacao_RoundTrip_ParticulasRicas()
        {
            var scene = new Scene();
            var o = new GameObject("Fx");
            o.AddComponent(new ParticleEmitter
            {
                EndColor = new Color(10, 20, 30), EndSize = 2f, BurstCount = 12, EmitOnStart = true
            });
            scene.Add(o);
            var pe = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene)).Objects.First()
                .Components.OfType<ParticleEmitter>().Single();
            Assert.AreEqual(new Color(10, 20, 30), pe.EndColor);
            Assert.AreEqual(2f, pe.EndSize);
            Assert.AreEqual(12, pe.BurstCount);
            Assert.IsTrue(pe.EmitOnStart);
        }
    }
}
