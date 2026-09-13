using System;
using System.IO;
using System.Text.Json;

namespace DreamBit.Studio
{
    /// <summary>
    /// Preferências do editor, persistidas em JSON no diretório de dados do usuário
    /// (cross-platform). Hoje guarda o caminho da engine (pasta que contém o
    /// DreamBit.Player), usada para rodar/exportar o jogo quando o editor não está
    /// dentro do repositório (ex.: instalado).
    /// </summary>
    public sealed class EditorPreferences
    {
        /// <summary>Pasta raiz da engine/repositório (contém DreamBit.Player). Null = auto.</summary>
        public string? EnginePath { get; set; }

        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        public static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DreamBit", "preferences.json");

        public static EditorPreferences Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<EditorPreferences>(File.ReadAllText(FilePath)) ?? new();
            }
            catch { /* preferências corrompidas: começa do zero */ }
            return new EditorPreferences();
        }

        public void Save()
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Options));
        }
    }

    /// <summary>
    /// Localiza a raiz da engine (pasta com o DreamBit.Player): usa a preferência do
    /// usuário se válida; senão sobe a árvore procurando a solução; senão null.
    /// </summary>
    public static class EngineLocator
    {
        /// <summary>Retorna a raiz da engine ou null se não encontrada.</summary>
        public static string? FindRoot()
        {
            var pref = EditorPreferences.Load().EnginePath;
            if (IsEngineRoot(pref))
                return pref;

            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "DreamBit.Studio.slnx")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        /// <summary>True se a pasta parece ser a raiz da engine (tem o projeto/exe do Player).</summary>
        public static bool IsEngineRoot(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return false;

            var playerDir = Path.Combine(path, "DreamBit.Player");
            if (File.Exists(Path.Combine(playerDir, "DreamBit.Player.csproj")))
                return true;

            // Ou uma pasta já publicada com o executável do Player.
            return File.Exists(Path.Combine(path, "DreamBit.Player.exe"))
                || File.Exists(Path.Combine(path, "DreamBit.Player.dll"));
        }
    }
}
