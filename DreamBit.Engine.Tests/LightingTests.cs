using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class LightingTests
    {
        [Fact]
        public void Serializacao_RoundTrip_LuzEAmbiente()
        {
            var scene = new Scene();
            var o = new GameObject("Tocha");
            o.AddComponent(new Light2D { Radius = 320f, Intensity = 1.4f, Color = new Color(255, 220, 180) });
            scene.Add(o);
            var a = new GameObject("Ambiente");
            a.AddComponent(new AmbientLight { Color = new Color(30, 34, 50) });
            scene.Add(a);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var light = loaded.Objects[0].Components.OfType<Light2D>().Single();
            Assert.Equal(320f, light.Radius);
            Assert.Equal(1.4f, light.Intensity, 0.001f);
            Assert.Equal(new Color(255, 220, 180), light.Color);
            var amb = loaded.Objects[1].Components.OfType<AmbientLight>().Single();
            Assert.Equal(new Color(30, 34, 50), amb.Color);
        }

        [Fact]
        public void Light2D_PodeSerDesligado()
        {
            var l = new Light2D();
            Assert.True(l.Enabled);
            l.Enabled = false;
            Assert.False(l.Enabled, "luz desligável (o renderer ignora luzes desligadas)");
        }
    }
}
