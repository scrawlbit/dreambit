using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class PrefabInstanceTests
    {
        [Fact]
        public void Serializacao_RoundTrip_PrefabInstance()
        {
            var scene = new Scene();
            var o = new GameObject("Inimigo");
            o.AddComponent(new PrefabInstance { PrefabPath = "prefabs/inimigo.dbprefab" });
            scene.Add(o);

            var e = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene)).Objects.First();
            var pi = e.Components.OfType<PrefabInstance>().Single();
            Assert.Equal("prefabs/inimigo.dbprefab", pi.PrefabPath);
        }
    }
}
