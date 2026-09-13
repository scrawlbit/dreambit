using System;
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
        public static void Launch(string scenePath)
        {
            var root = FindRepositoryRoot();
            if (root == null)
                throw new DirectoryNotFoundException("Raiz do repositório (DreamBit.Studio.slnx) não encontrada.");

            var playerDir = Path.Combine(root, "DreamBit.Player");
            var exe = FindPlayerExecutable(playerDir);

            if (exe != null)
            {
                Start(exe, Quote(scenePath));
                return;
            }

            // Fallback: compila e roda pelo SDK.
            var csproj = Path.Combine(playerDir, "DreamBit.Player.csproj");
            Start("dotnet", $"run --project {Quote(csproj)} -- {Quote(scenePath)}");
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
