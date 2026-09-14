using DreamBit.Engine.Elements;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Marca um objeto como instância de um prefab (arquivo .dbprefab). O editor usa isto para
    /// "Aplicar" (salvar as mudanças de volta no prefab) e "Reverter" (recarregar do arquivo).
    /// Base para prefabs reutilizáveis/aninhados — o próximo passo é o diff de overrides.
    /// </summary>
    public sealed class PrefabInstance : SceneComponent
    {
        private string _prefabPath = string.Empty;

        public override string DisplayName => "Prefab Instance";

        /// <summary>Caminho do arquivo .dbprefab de origem.</summary>
        public string PrefabPath { get => _prefabPath; set => Set(ref _prefabPath, value ?? string.Empty); }
    }
}
