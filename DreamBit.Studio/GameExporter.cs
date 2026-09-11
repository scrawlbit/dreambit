using System;
using System.Diagnostics;
using System.IO;

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

                File.WriteAllText(Path.Combine(outputDir, "game.dbscene"), sceneJson);

                return (true,
                    $"Jogo exportado em:\n{outputDir}\n\nRode o DreamBit.Player.exe de lá.\n" +
                    "Obs.: as imagens/sons são referenciados por caminho absoluto (não são copiados nesta versão).");
            }
            catch (Exception ex)
            {
                return (false, "Não foi possível exportar (precisa do SDK .NET).\n\n" + ex.Message);
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
