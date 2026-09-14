using System.Text.Json.Nodes;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class PrefabPatchTests
    {
        [TestMethod]
        public void Diff_CapturaSoOQueMudou()
        {
            var prefab = JsonNode.Parse("""{"Name":"Inimigo","Hp":100,"Speed":50}""")!;
            var inst = JsonNode.Parse("""{"Name":"Inimigo","Hp":80,"Speed":50}""")!;

            var patch = PrefabPatch.Diff(prefab, inst)!;
            Assert.IsNotNull(patch);
            Assert.AreEqual(80, (int)patch["Hp"]!);
            Assert.IsNull(patch["Speed"], "campo igual não entra no override");
            Assert.IsNull(patch["Name"]);
        }

        [TestMethod]
        public void Apply_PuxaPrefabNovoMantendoOverrides()
        {
            // instância mudou Hp para 80 (override).
            var prefabOld = JsonNode.Parse("""{"Hp":100,"Speed":50,"Color":"red"}""")!;
            var inst = JsonNode.Parse("""{"Hp":80,"Speed":50,"Color":"red"}""")!;
            var overrides = PrefabPatch.Diff(prefabOld, inst)!;

            // prefab foi atualizado upstream: Speed 60, Color blue.
            var prefabNew = JsonNode.Parse("""{"Hp":100,"Speed":60,"Color":"blue"}""")!;
            var merged = PrefabPatch.Apply(prefabNew, overrides)!;

            Assert.AreEqual(80, (int)merged["Hp"]!, "mantém o override local");
            Assert.AreEqual(60, (int)merged["Speed"]!, "puxa a mudança do prefab");
            Assert.AreEqual("blue", (string)merged["Color"]!, "puxa a mudança do prefab");
        }

        [TestMethod]
        public void Diff_Aninhado()
        {
            var prefab = JsonNode.Parse("""{"t":{"x":0,"y":0}}""")!;
            var inst = JsonNode.Parse("""{"t":{"x":10,"y":0}}""")!;
            var patch = PrefabPatch.Diff(prefab, inst)!;
            Assert.AreEqual(10, (int)patch["t"]!["x"]!);
            Assert.IsNull(patch["t"]!["y"]);
        }
    }
}
