using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DreamBit.Studio
{
    /// <summary>
    /// Localiza e inicia o executável DreamBit.Player para rodar uma cena.
    /// Procura o .exe já compilado; se não achar, cai para `dotnet run`.
    /// </summary>
    public static class PlayerLauncher
    {
        public static void Launch(string scenePath) => Launch(scenePath, null, null);

        /// <summary>Roda a cena no Player, carregando as DLLs de componentes do jogo
        /// (<c>--components</c>) e resolvendo assets relativos à pasta do projeto (<c>--assets</c>).</summary>
        public static void Launch(string scenePath, IEnumerable<string>? componentAssemblies, string? assetRoot)
        {
            var args = Quote(scenePath);
            if (!string.IsNullOrEmpty(assetRoot))
                args += " --assets " + Quote(assetRoot);
            foreach (var dll in componentAssemblies ?? Enumerable.Empty<string>())
                args += " --components " + Quote(dll);

            var root = EngineLocator.FindRoot();
            if (root == null)
                throw new DirectoryNotFoundException(
                    "Engine não encontrada. Defina a pasta da engine em Preferências (menu).");

            // Pasta já publicada: o executável fica direto na raiz.
            var direct = Path.Combine(root, "DreamBit.Player.exe");
            if (File.Exists(direct))
            {
                Start(direct, args);
                return;
            }

            var playerDir = Path.Combine(root, "DreamBit.Player");
            var exe = FindPlayerExecutable(playerDir);
            if (exe != null)
            {
                Start(exe, args);
                return;
            }

            // Fallback: compila e roda pelo SDK.
            var csproj = Path.Combine(playerDir, "DreamBit.Player.csproj");
            Start("dotnet", $"run --project {Quote(csproj)} -- {args}");
        }

        private static string? FindPlayerExecutable(string playerDir)
        {
            var bin = Path.Combine(playerDir, "bin");
            if (!Directory.Exists(bin))
                return null;

            return Directory
                .EnumerateFiles(bin, "DreamBit.Player.exe", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }

        private static void Start(string fileName, string arguments)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = true
            });
        }

        private static string Quote(string value) => "\"" + value + "\"";
    }
}
