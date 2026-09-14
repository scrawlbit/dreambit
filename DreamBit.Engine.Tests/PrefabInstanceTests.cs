using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class PrefabInstanceTests
    {
        [TestMethod]
        public void Serializacao_RoundTrip_PrefabInstance()
        {
            var scene = new Scene();
            var o = new GameObject("Inimigo");
            o.AddComponent(new PrefabInstance { PrefabPath = "prefabs/inimigo.dbprefab" });
            scene.Add(o);

            var e = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene)).Objects.First();
            var pi = e.Components.OfType<PrefabInstance>().Single();
            Assert.AreEqual("prefabs/inimigo.dbprefab", pi.PrefabPath);
        }
    }
}
