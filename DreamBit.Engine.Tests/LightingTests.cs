using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class LightingTests
    {
        [TestMethod]
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
            Assert.AreEqual(320f, light.Radius);
            Assert.AreEqual(1.4f, light.Intensity, 0.001f);
            Assert.AreEqual(new Color(255, 220, 180), light.Color);
            var amb = loaded.Objects[1].Components.OfType<AmbientLight>().Single();
            Assert.AreEqual(new Color(30, 34, 50), amb.Color);
        }

        [TestMethod]
        public void Light2D_PodeSerDesligado()
        {
            var l = new Light2D();
            Assert.IsTrue(l.Enabled);
            l.Enabled = false;
            Assert.IsFalse(l.Enabled, "luz desligável (o renderer ignora luzes desligadas)");
        }
    }
}
