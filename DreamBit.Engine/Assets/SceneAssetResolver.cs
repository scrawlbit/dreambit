using System;
using System.Collections.Generic;
using System.IO;
using DreamBit.Engine.Serialization;

namespace DreamBit.Engine.Assets
{
    /// <summary>
    /// Faz uma cena referenciar assets por identidade (GUID) em vez de só por caminho: ao salvar,
    /// "carimba" o GUID de cada asset referenciado; ao carregar, se um caminho quebrou (asset
    /// movido/renomeado), reencontra o arquivo pelo GUID via <see cref="AssetDatabase"/>. Cobre os
    /// campos de caminho conhecidos (textura, som, tmx, tileset, script, prefab).
    /// </summary>
    public static class SceneAssetResolver
    {
        /// <summary>Grava o GUID de cada asset referenciado no mapa da cena (SceneData.Assets).</summary>
        public static void Stamp(SceneData scene, AssetDatabase db, string projectRoot)
        {
            scene.Assets.Clear();
            foreach (var (path, _) in Refs(scene))
            {
                if (string.IsNullOrEmpty(path)) continue;
                var abs = Absolute(path, projectRoot);
                if (!File.Exists(abs)) continue;
                scene.Assets[path] = db.GuidForFile(abs).ToString();
            }
        }

        /// <summary>Reescreve os caminhos quebrados para a localização atual do asset (por GUID).
        /// Retorna quantos caminhos foram corrigidos.</summary>
        public static int Resolve(SceneData scene, AssetDatabase db, string projectRoot)
        {
            int fixedCount = 0;
            foreach (var (path, set) in Refs(scene))
            {
                if (string.IsNullOrEmpty(path)) continue;
                if (File.Exists(Absolute(path, projectRoot))) continue; // ainda válido
                if (!scene.Assets.TryGetValue(path, out var guidStr) || !Guid.TryParse(guidStr, out var guid))
                    continue;
                var current = db.PathForGuid(guid);
                if (current == null) continue;
                set(Relative(current, projectRoot));
                fixedCount++;
            }
            return fixedCount;
        }

        private static string Absolute(string path, string root)
            => Path.IsPathRooted(path) ? path : Path.Combine(root, path);

        private static string Relative(string absolute, string root)
        {
            try { return Path.GetRelativePath(root, absolute).Replace('\\', '/'); }
            catch { return absolute; }
        }

        // Todos os campos de caminho de asset da cena (get + setter), percorrendo os filhos.
        private static IEnumerable<(string Path, Action<string> Set)> Refs(SceneData scene)
        {
            foreach (var obj in scene.Objects)
                foreach (var r in Refs(obj))
                    yield return r;
        }

        private static IEnumerable<(string Path, Action<string> Set)> Refs(GameObjectData obj)
        {
            foreach (var sp in obj.Sprites)
                yield return (sp.TexturePath ?? "", v => sp.TexturePath = v);
            foreach (var an in obj.Animators)
                yield return (an.TexturePath ?? "", v => an.TexturePath = v);
            foreach (var au in obj.Audios)
                yield return (au.SoundPath ?? "", v => au.SoundPath = v);
            foreach (var tm in obj.Tilemaps)
            {
                yield return (tm.TmxPath ?? "", v => tm.TmxPath = v);
                foreach (var ts in tm.Tilesets)
                    yield return (ts.ImagePath ?? "", v => ts.ImagePath = v);
            }
            foreach (var sc in obj.Scripts)
                yield return (sc.SourcePath ?? "", v => sc.SourcePath = v);
            foreach (var pi in obj.PrefabInstances)
                yield return (pi.PrefabPath ?? "", v => pi.PrefabPath = v);

            foreach (var child in obj.Children)
                foreach (var r in Refs(child))
                    yield return r;
        }
    }
}
