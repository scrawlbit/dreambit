using System.Text.Json.Nodes;

namespace DreamBit.Engine.Serialization
{
    /// <summary>
    /// Diff/merge de JSON (estilo merge-patch, RFC 7386) para overrides de prefab: <see cref="Diff"/>
    /// calcula o que a instância mudou em relação ao prefab; <see cref="Apply"/> reaplica essas
    /// mudanças sobre uma versão (possivelmente nova) do prefab — permitindo puxar atualizações do
    /// prefab mantendo as edições locais da instância.
    /// </summary>
    public static class PrefabPatch
    {
        /// <summary>Overrides da instância em relação ao prefab (só o que difere). Null = idênticos.</summary>
        public static JsonNode? Diff(JsonNode? prefab, JsonNode? instance)
        {
            if (prefab is JsonObject pObj && instance is JsonObject iObj)
            {
                var patch = new JsonObject();
                foreach (var kv in iObj)
                {
                    var sub = Diff(pObj[kv.Key], kv.Value);
                    if (sub != null || !pObj.ContainsKey(kv.Key))
                        patch[kv.Key] = sub ?? Clone(kv.Value);
                }
                foreach (var kv in pObj)
                    if (!iObj.ContainsKey(kv.Key))
                        patch[kv.Key] = null; // removido na instância
                return patch.Count > 0 ? patch : null;
            }

            return Equal(prefab, instance) ? null : Clone(instance);
        }

        /// <summary>Aplica um patch de overrides sobre uma base (prefab), retornando o resultado mesclado.</summary>
        public static JsonNode? Apply(JsonNode? baseNode, JsonNode? patch)
        {
            if (baseNode is JsonObject bObj && patch is JsonObject pObj)
            {
                var result = (JsonObject)Clone(bObj)!;
                foreach (var kv in pObj)
                {
                    if (kv.Value == null)
                        result.Remove(kv.Key);
                    else if (result[kv.Key] is JsonObject && kv.Value is JsonObject)
                        result[kv.Key] = Apply(result[kv.Key], kv.Value);
                    else
                        result[kv.Key] = Clone(kv.Value);
                }
                return result;
            }
            return Clone(patch);
        }

        private static bool Equal(JsonNode? a, JsonNode? b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.ToJsonString() == b.ToJsonString();
        }

        private static JsonNode? Clone(JsonNode? node) => node?.DeepClone();
    }
}
