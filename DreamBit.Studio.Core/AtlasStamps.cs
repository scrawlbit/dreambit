using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DreamBit.Studio
{
    /// <summary>Um carimbo nomeado recortado de um atlas: nome + retângulo (px) na folha.</summary>
    public sealed class AtlasStamp
    {
        public string Name { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    /// <summary>
    /// Coleção de carimbos recortados de um atlas, persistida ao lado do PNG em
    /// <c>&lt;atlas&gt;.stamps.json</c>. Assim os recortes são guardados e reutilizados:
    /// reabrir o atlas recarrega os carimbos para só selecionar.
    /// </summary>
    public sealed class AtlasStampLibrary
    {
        public List<AtlasStamp> Stamps { get; set; } = new();

        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        public static string SidecarPath(string atlasPath) => atlasPath + ".stamps.json";

        public static AtlasStampLibrary Load(string atlasPath)
        {
            try
            {
                var path = SidecarPath(atlasPath);
                if (File.Exists(path))
                    return JsonSerializer.Deserialize<AtlasStampLibrary>(File.ReadAllText(path)) ?? new AtlasStampLibrary();
            }
            catch { /* json inválido: começa vazio */ }
            return new AtlasStampLibrary();
        }

        public void Save(string atlasPath)
        {
            try { File.WriteAllText(SidecarPath(atlasPath), JsonSerializer.Serialize(this, Options)); }
            catch { /* disco indisponível: ignora */ }
        }

        /// <summary>Gera um nome único "carimbo N" que ainda não existe na coleção.</summary>
        public string NextName(string prefix = "carimbo")
        {
            int n = Stamps.Count + 1;
            string name;
            do { name = $"{prefix} {n++}"; }
            while (Stamps.Exists(s => s.Name == name));
            return name;
        }
    }
}
