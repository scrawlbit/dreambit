using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace DreamBit.Engine.Saving
{
    /// <summary>
    /// Progresso persistente do jogo (save game): um dicionário chave→valor tipado, salvo em
    /// JSON. Pensado para ser usado por scripts (<c>SaveGame.SetInt("fase", 2)</c>,
    /// <c>SaveGame.Save()</c>) — como <c>PlayerPrefs</c> do Unity ou um <c>Resource</c> de
    /// save do Godot. Simples e sem dependências: um único arquivo por perfil.
    /// </summary>
    public static class SaveGame
    {
        private static readonly Dictionary<string, string> _data = new(StringComparer.Ordinal);

        /// <summary>Caminho padrão do arquivo de save (na pasta de dados do usuário).</summary>
        public static string DefaultPath
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "DreamBit");
                return Path.Combine(dir, "savegame.json");
            }
        }

        // ---- leitura/escrita tipada ----

        public static void SetString(string key, string value) => _data[key] = value ?? string.Empty;
        public static void SetInt(string key, int value) => _data[key] = value.ToString(CultureInfo.InvariantCulture);
        public static void SetFloat(string key, float value) => _data[key] = value.ToString(CultureInfo.InvariantCulture);
        public static void SetBool(string key, bool value) => _data[key] = value ? "1" : "0";

        public static string GetString(string key, string fallback = "")
            => _data.TryGetValue(key, out var v) ? v : fallback;

        public static int GetInt(string key, int fallback = 0)
            => _data.TryGetValue(key, out var v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : fallback;

        public static float GetFloat(string key, float fallback = 0f)
            => _data.TryGetValue(key, out var v) && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var r) ? r : fallback;

        public static bool GetBool(string key, bool fallback = false)
            => _data.TryGetValue(key, out var v) ? v == "1" || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) : fallback;

        public static bool Has(string key) => _data.ContainsKey(key);
        public static void Delete(string key) => _data.Remove(key);
        public static void Clear() => _data.Clear();

        /// <summary>Instantâneo atual (para inspeção/testes).</summary>
        public static IReadOnlyDictionary<string, string> Snapshot() => new Dictionary<string, string>(_data);

        // ---- persistência ----

        /// <summary>Grava no arquivo (padrão <see cref="DefaultPath"/>).</summary>
        public static void Save(string? path = null)
        {
            path ??= DefaultPath;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonSerializer.Serialize(_data));
        }

        /// <summary>Carrega do arquivo, substituindo o estado atual. Retorna false se não existir.</summary>
        public static bool Load(string? path = null)
        {
            path ??= DefaultPath;
            if (!File.Exists(path))
                return false;

            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
            _data.Clear();
            if (loaded != null)
                foreach (var kv in loaded)
                    _data[kv.Key] = kv.Value;
            return true;
        }

        /// <summary>True se existe um arquivo de save no caminho informado (ou no padrão).</summary>
        public static bool Exists(string? path = null) => File.Exists(path ?? DefaultPath);
    }
}
