using System;
using System.Diagnostics;
using System.IO;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;

namespace DreamBit.Studio
{
    /// <summary>
    /// Exporta o jogo: publica o DreamBit.Player numa pasta e inclui a cena atual
    /// como "game.dbscene" (o Player a carrega automaticamente).
    /// </summary>
    internal static class GameExporter
    {
        public static (bool Ok, string Message) Publish(string outputDir, string sceneJson)
        {
            var root = FindRepositoryRoot();
            if (root == null)
                return (false, "Raiz do repositório (DreamBit.Studio.slnx) não encontrada.");

            var csproj = Path.Combine(root, "DreamBit.Player", "DreamBit.Player.csproj");

            try
            {
                Directory.CreateDirectory(outputDir);

                var info = new ProcessStartInfo("dotnet",
                    $"publish \"{csproj}\" -c Release -o \"{outputDir}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(info)!;
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit(600000);

                if (process.ExitCode != 0)
                    return (false, $"Falha ao publicar (código {process.ExitCode}).\n\n{Tail(output + error)}");

                // Copia os assets referenciados e reescreve os caminhos para relativos.
                var scene = SceneSerializer.LoadFromString(sceneJson);
                BundleAssets(scene, outputDir);
                File.WriteAllText(Path.Combine(outputDir, "game.dbscene"), SceneSerializer.SaveToString(scene));

                return (true,
                    $"Jogo exportado (com assets) em:\n{outputDir}\n\nRode o DreamBit.Player.exe de lá — a pasta é portátil.");
            }
            catch (Exception ex)
            {
                return (false, "Não foi possível exportar (precisa do SDK .NET).\n\n" + ex.Message);
            }
        }

        private static void BundleAssets(Scene scene, string outputDir)
        {
            var assetsDir = Path.Combine(outputDir, "assets");
            Directory.CreateDirectory(assetsDir);

            foreach (var obj in Flatten(scene.Objects))
            {
                foreach (var component in obj.Components)
                {
                    switch (component)
                    {
                        case SpriteRenderer s:
                            s.TexturePath = Copy(s.TexturePath, assetsDir);
                            break;
                        case SpriteAnimator a:
                            a.TexturePath = Copy(a.TexturePath, assetsDir);
                            break;
                        case AudioSource au:
                            au.SoundPath = Copy(au.SoundPath, assetsDir);
                            break;
                        case TilemapRenderer t:
                            var map = t.Map;
                            if (map != null)
                            {
                                t.Edited = true; // força serialização inline com os caminhos novos
                                foreach (var ts in map.Tilesets)
                                    ts.ResolvedImagePath = Copy(ts.ResolvedImagePath, assetsDir);
                            }
                            break;
                    }
                }
            }
        }

        private static string? Copy(string? path, string assetsDir)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            string src = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
            if (!File.Exists(src))
                return path;

            string name = Path.GetFileName(src);
            try { File.Copy(src, Path.Combine(assetsDir, name), true); }
            catch { return path; }

            return "assets/" + name;
        }

        private static System.Collections.Generic.IEnumerable<GameObject> Flatten(System.Collections.Generic.IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in Flatten(obj.Children))
                    yield return child;
            }
        }

        private static string? FindRepositoryRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "DreamBit.Studio.slnx")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        private static string Tail(string text) => text.Length > 1500 ? "…" + text[^1500..] : text;
    }
}
