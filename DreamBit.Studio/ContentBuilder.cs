using System;
using System.Diagnostics;
using DreamBit.Engine.Project;

namespace DreamBit.Studio
{
    /// <summary>
    /// Gera o Content.mgcb do projeto e invoca o `dotnet mgcb` para compilar os assets.
    /// Degrada graciosamente quando a ferramenta não está instalada.
    /// </summary>
    internal static class ContentBuilder
    {
        public static (bool Ok, string Message) Build(GameProject project)
        {
            project.WriteContentManifest();

            try
            {
                var info = new ProcessStartInfo("dotnet", $"mgcb /@:\"{ContentManifest.FileName}\"")
                {
                    WorkingDirectory = project.Folder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(info)!;
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit(120000);

                return process.ExitCode == 0
                    ? (true, $"{ContentManifest.FileName} gerado e conteúdo compilado.\n\n{Tail(output)}")
                    : (false, $"Falha no MGCB (código {process.ExitCode}).\n\n{Tail(output + error)}");
            }
            catch (Exception ex)
            {
                return (false,
                    $"{ContentManifest.FileName} gerado, mas não foi possível executar 'dotnet mgcb'.\n" +
                    "Instale a ferramenta com:\n\n  dotnet tool install -g dotnet-mgcb\n\n" + ex.Message);
            }
        }

        private static string Tail(string text)
            => text.Length > 1200 ? "…" + text[^1200..] : text;
    }
}
