using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DreamBit.Studio
{
    /// <summary>
    /// Preferências do editor, persistidas em JSON no diretório de dados do usuário
    /// (cross-platform). Guarda o caminho da engine e o mapa de atalhos (teclas de atalho
    /// configuráveis). Pode ser exportado/importado para compartilhar com outros PCs.
    /// </summary>
    public sealed class EditorPreferences
    {
        /// <summary>Pasta raiz da engine/repositório (contém DreamBit.Player). Null = auto.</summary>
        public string? EnginePath { get; set; }

        /// <summary>Programa (IDE) usado para abrir scripts .cs — ex.: Rider, Visual Studio, VS Code.
        /// Null = detecta na primeira vez e guarda a escolha.</summary>
        public string? ScriptEditorPath { get; set; }

        /// <summary>Atlases abertos recentemente no seletor de carimbos (mais recente primeiro).</summary>
        public List<string> RecentAtlases { get; set; } = new();

        /// <summary>Registra um atlas como recente (dedup, mais recente primeiro, máx. 10) e salva.</summary>
        public void PushRecentAtlas(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;
            RecentAtlases.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            RecentAtlases.Insert(0, path);
            if (RecentAtlases.Count > 10)
                RecentAtlases.RemoveRange(10, RecentAtlases.Count - 10);
            Save();
        }

        /// <summary>Atalhos: ação → gesto (ex.: "Group" → "Ctrl+G"). Falhas caem no default.</summary>
        public Dictionary<string, string> Shortcuts { get; set; } = new();

        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        /// <summary>Ações com atalho e o rótulo exibido nas preferências (ordem estável).</summary>
        public static readonly (string Action, string Label)[] KnownActions =
        {
            ("Group", "Agrupar"),
            ("Ungroup", "Desagrupar"),
            ("Undo", "Desfazer"),
            ("Redo", "Refazer"),
            ("Duplicate", "Duplicar"),
            ("Copy", "Copiar"),
            ("Paste", "Colar"),
            ("Delete", "Excluir"),
            ("Save", "Salvar cena"),
            ("Play", "Play/Pause"),
            ("RunPlayer", "Rodar no Player"),
            ("Atlas", "Abrir Atlas (carimbos)"),
            ("ZoomReset", "Zoom 100%"),
            ("FocusSelection", "Focar seleção"),
        };

        /// <summary>Atalhos padrão de fábrica.</summary>
        public static Dictionary<string, string> DefaultShortcuts() => new()
        {
            ["Group"] = "Ctrl+G",
            ["Ungroup"] = "Ctrl+Shift+G",
            ["Undo"] = "Ctrl+Z",
            ["Redo"] = "Ctrl+Y",
            ["Duplicate"] = "Ctrl+D",
            ["Copy"] = "Ctrl+C",
            ["Paste"] = "Ctrl+V",
            ["Delete"] = "Delete",
            ["Save"] = "Ctrl+S",
            ["Play"] = "F5",
            ["RunPlayer"] = "Ctrl+F5",
            ["Atlas"] = "Ctrl+B",
            ["ZoomReset"] = "Ctrl+D0",
            ["FocusSelection"] = "F",
        };

        /// <summary>Gesto atual de uma ação (o do usuário ou o padrão).</summary>
        public string GestureFor(string action)
            => Shortcuts.TryGetValue(action, out var g) && !string.IsNullOrWhiteSpace(g)
                ? g : DefaultShortcuts().GetValueOrDefault(action, "");

        /// <summary>Ação que corresponde a um gesto (ex.: "Ctrl+G" → "Group"), ou null.</summary>
        public string? ActionFor(string gesture)
        {
            foreach (var (action, _) in KnownActions)
                if (string.Equals(GestureFor(action), gesture, StringComparison.OrdinalIgnoreCase))
                    return action;
            return null;
        }

        /// <summary>Restaura os atalhos de fábrica.</summary>
        public void ResetShortcuts() => Shortcuts = DefaultShortcuts();

        public static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DreamBit", "preferences.json");

        public static EditorPreferences Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var prefs = JsonSerializer.Deserialize<EditorPreferences>(File.ReadAllText(FilePath)) ?? new();
                    prefs.FillDefaults();
                    return prefs;
                }
            }
            catch { /* preferências corrompidas: começa do zero */ }
            var fresh = new EditorPreferences();
            fresh.FillDefaults();
            return fresh;
        }

        public void Save()
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Options));
        }

        /// <summary>Exporta as preferências (inclui atalhos) para um arquivo, p/ outro PC.</summary>
        public void ExportTo(string path)
            => File.WriteAllText(path, JsonSerializer.Serialize(this, Options));

        /// <summary>Importa preferências de um arquivo exportado.</summary>
        public static EditorPreferences ImportFrom(string path)
        {
            var prefs = JsonSerializer.Deserialize<EditorPreferences>(File.ReadAllText(path)) ?? new();
            prefs.FillDefaults();
            return prefs;
        }

        /// <summary>Garante um gesto para toda ação conhecida (preenche faltantes com o padrão).</summary>
        public void FillDefaults()
        {
            var def = DefaultShortcuts();
            Shortcuts ??= new();
            foreach (var (action, _) in KnownActions)
                if (!Shortcuts.ContainsKey(action) || string.IsNullOrWhiteSpace(Shortcuts[action]))
                    Shortcuts[action] = def.GetValueOrDefault(action, "");
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
