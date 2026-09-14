using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DreamBit.Engine.Assets
{
    /// <summary>
    /// Banco de assets com identidade estável: cada arquivo ganha um GUID guardado num sidecar
    /// "<arquivo>.meta". Referenciar por GUID (em vez de caminho) sobrevive a renomear/mover o
    /// asset — o <see cref="PathForGuid"/> reencontra o arquivo pelo .meta. Base para o sistema
    /// de referências por ID (como os .meta do Unity / UID do Godot). Editor-agnóstico e testável.
    /// </summary>
    public sealed class AssetDatabase
    {
        private sealed class Meta { public string Guid { get; set; } = ""; }

        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
        private readonly string _root;
        private readonly Dictionary<Guid, string> _byGuid = new();

        public AssetDatabase(string rootFolder)
        {
            _root = rootFolder;
            Refresh();
        }

        /// <summary>Extensões consideradas assets (ignora .meta e temporários).</summary>
        public static readonly HashSet<string> AssetExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".wav", ".ogg", ".tmx", ".tsx", ".dbprefab", ".dbscene", ".cs", ".json"
        };

        /// <summary>Relê todos os .meta sob a raiz (chame quando os arquivos mudam).</summary>
        public void Refresh()
        {
            _byGuid.Clear();
            if (!Directory.Exists(_root))
                return;

            foreach (var meta in Directory.EnumerateFiles(_root, "*.meta", SearchOption.AllDirectories))
            {
                var asset = meta.Substring(0, meta.Length - ".meta".Length);
                if (!File.Exists(asset))
                    continue; // .meta órfão (asset removido)
                if (TryReadGuid(meta, out var guid))
                    _byGuid[guid] = asset;
            }
        }

        /// <summary>GUID de um asset (cria o .meta se ainda não existir). Caminho absoluto.</summary>
        public Guid GuidForFile(string assetPath)
        {
            var metaPath = assetPath + ".meta";
            if (File.Exists(metaPath) && TryReadGuid(metaPath, out var existing))
            {
                _byGuid[existing] = assetPath;
                return existing;
            }

            var guid = Guid.NewGuid();
            File.WriteAllText(metaPath, JsonSerializer.Serialize(new Meta { Guid = guid.ToString() }, Options));
            _byGuid[guid] = assetPath;
            return guid;
        }

        /// <summary>Caminho atual do asset com este GUID, ou null se não encontrado.</summary>
        public string? PathForGuid(Guid guid)
        {
            if (_byGuid.TryGetValue(guid, out var path) && File.Exists(path))
                return path;
            Refresh(); // pode ter sido movido: reindexa e tenta de novo
            return _byGuid.TryGetValue(guid, out var again) && File.Exists(again) ? again : null;
        }

        private static bool TryReadGuid(string metaPath, out Guid guid)
        {
            guid = Guid.Empty;
            try
            {
                var meta = JsonSerializer.Deserialize<Meta>(File.ReadAllText(metaPath));
                return meta != null && Guid.TryParse(meta.Guid, out guid);
            }
            catch { return false; }
        }
    }
}
