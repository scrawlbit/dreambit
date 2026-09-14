using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class ParticleTests
    {
        [Fact]
        public void Burst_EmiteVariasDeUmaVez()
        {
            var scene = new Scene();
            var o = new GameObject("Fx");
            var pe = new ParticleEmitter { EmitRate = 0f, BurstCount = 20, EmitOnStart = true, Lifetime = 2f };
            o.AddComponent(pe);
            scene.Add(o);
            scene.StartPlay();
            Assert.Equal(20, pe.ActiveParticles);

            pe.Burst(5);
            Assert.Equal(25, pe.ActiveParticles);
        }

        [Fact]
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
            Assert.Equal(new Color(10, 20, 30), pe.EndColor);
            Assert.Equal(2f, pe.EndSize);
            Assert.Equal(12, pe.BurstCount);
            Assert.True(pe.EmitOnStart);
        }
    }
}
