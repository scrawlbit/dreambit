using System.Text.Json.Nodes;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class PrefabPatchTests
    {
        [Fact]
        public void Diff_CapturaSoOQueMudou()
        {
            var prefab = JsonNode.Parse("""{"Name":"Inimigo","Hp":100,"Speed":50}""")!;
            var inst = JsonNode.Parse("""{"Name":"Inimigo","Hp":80,"Speed":50}""")!;

            var patch = PrefabPatch.Diff(prefab, inst)!;
            Assert.NotNull(patch);
            Assert.Equal(80, (int)patch["Hp"]!);
            Assert.Null(patch["Speed"]);
            Assert.Null(patch["Name"]);
        }

        [Fact]
        public void Apply_PuxaPrefabNovoMantendoOverrides()
        {
            // instância mudou Hp para 80 (override).
            var prefabOld = JsonNode.Parse("""{"Hp":100,"Speed":50,"Color":"red"}""")!;
            var inst = JsonNode.Parse("""{"Hp":80,"Speed":50,"Color":"red"}""")!;
            var overrides = PrefabPatch.Diff(prefabOld, inst)!;

            // prefab foi atualizado upstream: Speed 60, Color blue.
            var prefabNew = JsonNode.Parse("""{"Hp":100,"Speed":60,"Color":"blue"}""")!;
            var merged = PrefabPatch.Apply(prefabNew, overrides)!;

            Assert.Equal(80, (int)merged["Hp"]!);
            Assert.Equal(60, (int)merged["Speed"]!);
            Assert.Equal("blue", (string)merged["Color"]!);
        }

        [Fact]
        public void Diff_Aninhado()
        {
            var prefab = JsonNode.Parse("""{"t":{"x":0,"y":0}}""")!;
            var inst = JsonNode.Parse("""{"t":{"x":10,"y":0}}""")!;
            var patch = PrefabPatch.Diff(prefab, inst)!;
            Assert.Equal(10, (int)patch["t"]!["x"]!);
            Assert.Null(patch["t"]!["y"]);
        }
    }
}
