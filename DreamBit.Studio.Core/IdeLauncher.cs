using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DreamBit.Studio
{
    /// <summary>
    /// Abre scripts .cs numa IDE (Rider, Visual Studio ou VS Code): detecta a IDE instalada,
    /// procura a solução mais próxima e abre primeiro a solução e depois o arquivo. O programa
    /// escolhido é guardado em <see cref="EditorPreferences.ScriptEditorPath"/>.
    /// </summary>
    public static class IdeLauncher
    {
        public enum IdeKind { VsCode, Rider, VisualStudio, Other }

        public static IdeKind Classify(string exe)
        {
            var name = Path.GetFileNameWithoutExtension(exe).ToLowerInvariant();
            if (name.Contains("rider")) return IdeKind.Rider;
            if (name.Contains("devenv")) return IdeKind.VisualStudio;
            if (name == "code" || name.Contains("vscode")) return IdeKind.VsCode;
            return IdeKind.Other;
        }

        /// <summary>Procura uma IDE instalada (ordem: Rider, Visual Studio, VS Code). Null se nada achado.</summary>
        public static string? AutoDetect()
        {
            foreach (var c in Candidates())
                if (File.Exists(c))
                    return c;

            // Também tenta no PATH (ex.: "code", "rider").
            foreach (var name in new[] { "rider64", "rider", "devenv", "code" })
            {
                var onPath = OnPath(name);
                if (onPath != null)
                    return onPath;
            }
            return null;
        }

        private static System.Collections.Generic.IEnumerable<string> Candidates()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pfx = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            // Rider (JetBrains Toolbox e instalação padrão).
            string toolbox = Path.Combine(local, "Programs");
            if (Directory.Exists(toolbox))
                foreach (var dir in SafeDirs(toolbox, "*ider*"))
                {
                    var exe = Path.Combine(dir, "bin", "rider64.exe");
                    if (File.Exists(exe)) yield return exe;
                }
            var jb = Path.Combine(pf, "JetBrains");
            if (Directory.Exists(jb))
                foreach (var dir in SafeDirs(jb, "*ider*"))
                {
                    var exe = Path.Combine(dir, "bin", "rider64.exe");
                    if (File.Exists(exe)) yield return exe;
                }

            // Visual Studio (via vswhere).
            var vs = FindVisualStudio(pfx);
            if (vs != null) yield return vs;

            // VS Code.
            yield return Path.Combine(local, "Programs", "Microsoft VS Code", "Code.exe");
            yield return Path.Combine(pf, "Microsoft VS Code", "Code.exe");
        }

        private static System.Collections.Generic.IEnumerable<string> SafeDirs(string root, string pattern)
        {
            string[] dirs;
            try { dirs = Directory.GetDirectories(root, pattern); }
            catch { yield break; }
            foreach (var d in dirs) yield return d;
        }

        private static string? FindVisualStudio(string programFilesX86)
        {
            var vswhere = Path.Combine(programFilesX86, "Microsoft Visual Studio", "Installer", "vswhere.exe");
            if (!File.Exists(vswhere))
                return null;
            try
            {
                var psi = new ProcessStartInfo(vswhere,
                    "-latest -products * -property productPath")
                { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
                using var p = Process.Start(psi);
                if (p == null) return null;
                var outp = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit(3000);
                return File.Exists(outp) ? outp : null;
            }
            catch { return null; }
        }

        private static string? OnPath(string name)
        {
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
            foreach (var dir in paths)
                foreach (var ext in new[] { ".exe", ".cmd", ".bat", "" })
                {
                    string candidate;
                    try { candidate = Path.Combine(dir, name + ext); }
                    catch { continue; }
                    if (File.Exists(candidate)) return candidate;
                }
            return null;
        }

        /// <summary>Solução mais próxima (.slnx ou .sln) subindo a partir do arquivo; null se nenhuma.</summary>
        public static string? FindSolution(string file)
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(file))!);
            while (dir != null)
            {
                var sln = dir.GetFiles("*.slnx").Concat(dir.GetFiles("*.sln")).FirstOrDefault();
                if (sln != null) return sln.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        /// <summary>Abre a solução (se houver) e depois o arquivo na IDE informada.</summary>
        public static void Open(string editorExe, string file)
        {
            var kind = Classify(editorExe);
            var solution = FindSolution(file);
            var folder = solution != null ? Path.GetDirectoryName(solution)! : Path.GetDirectoryName(Path.GetFullPath(file))!;

            switch (kind)
            {
                case IdeKind.VsCode:
                    // Abre a pasta (contexto) e o arquivo numa chamada só.
                    Start(editorExe, $"\"{folder}\" -g \"{file}\"");
                    break;
                case IdeKind.Rider:
                    if (solution != null) Start(editorExe, $"\"{solution}\"");
                    Start(editorExe, $"\"{file}\"");
                    break;
                case IdeKind.VisualStudio:
                    if (solution != null) Start(editorExe, $"\"{solution}\"");
                    Start(editorExe, $"/Edit \"{file}\"");
                    break;
                default:
                    Start(editorExe, $"\"{file}\"");
                    break;
            }
        }

        private static void Start(string exe, string args)
        {
            try { Process.Start(new ProcessStartInfo(exe, args) { UseShellExecute = true }); }
            catch { /* IDE indisponível: silencioso (o chamador pode abrir com o shell) */ }
        }

        /// <summary>Template de um script novo (classe com o nome do arquivo).</summary>
        public static string ScriptTemplate(string className)
        {
            string safe = SanitizeClassName(className);
            return
                "using DreamBit.Engine.Elements;\n" +
                "using DreamBit.Engine.Scripting;\n\n" +
                $"public class {safe} : IGameScript\n" +
                "{\n" +
                "    public void Update(GameObject self, float dt)\n" +
                "    {\n" +
                "        // Roda a cada frame no play. Ex.: girar o objeto.\n" +
                "        self.Transform.Rotation += 1.5f * dt;\n" +
                "    }\n" +
                "}\n";
        }

        public static string SanitizeClassName(string name)
        {
            var chars = name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
            var s = new string(chars);
            if (string.IsNullOrEmpty(s) || char.IsDigit(s[0])) s = "Script" + s;
            return s;
        }
    }
}
