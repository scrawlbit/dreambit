using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DreamBit.Engine.Localization
{
    /// <summary>
    /// Tabela de textos por idioma: registra tabelas chave→texto e resolve pela linguagem
    /// atual, com fallback para a chave. Pensada para menus/HUD/diálogos —
    /// <c>Localizer.Get("play")</c>. Simples e sem dependências (um JSON por idioma).
    /// </summary>
    public static class Localizer
    {
        // idioma -> (chave -> texto)
        private static readonly Dictionary<string, Dictionary<string, string>> _tables =
            new(StringComparer.OrdinalIgnoreCase);

        private static string _language = "pt";

        /// <summary>Idioma atual (ex.: "pt", "en"). Trocar reflete nos próximos Get.</summary>
        public static string Language
        {
            get => _language;
            set => _language = string.IsNullOrWhiteSpace(value) ? _language : value;
        }

        /// <summary>Idioma de fallback quando a chave não existe no atual.</summary>
        public static string FallbackLanguage { get; set; } = "en";

        /// <summary>Registra (mescla) uma tabela de textos para um idioma.</summary>
        public static void Load(string language, IReadOnlyDictionary<string, string> table)
        {
            if (string.IsNullOrWhiteSpace(language) || table == null)
                return;
            if (!_tables.TryGetValue(language, out var dict))
                _tables[language] = dict = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var kv in table)
                dict[kv.Key] = kv.Value;
        }

        /// <summary>Carrega uma tabela JSON (objeto chave→texto) para um idioma.</summary>
        public static void LoadJson(string language, string json)
        {
            var table = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (table != null)
                Load(language, table);
        }

        /// <summary>Carrega uma tabela JSON de arquivo. Retorna false se não existir.</summary>
        public static bool LoadFile(string language, string path)
        {
            if (!File.Exists(path))
                return false;
            LoadJson(language, File.ReadAllText(path));
            return true;
        }

        /// <summary>Texto da chave no idioma atual; cai no fallback e, por fim, na própria chave.</summary>
        public static string Get(string key, string? fallback = null)
        {
            if (string.IsNullOrEmpty(key))
                return fallback ?? string.Empty;

            if (_tables.TryGetValue(_language, out var dict) && dict.TryGetValue(key, out var v))
                return v;
            if (_tables.TryGetValue(FallbackLanguage, out var fb) && fb.TryGetValue(key, out var v2))
                return v2;
            return fallback ?? key;
        }

        public static bool Has(string key)
            => _tables.TryGetValue(_language, out var dict) && dict.ContainsKey(key);

        /// <summary>Remove todas as tabelas (para testes/reset).</summary>
        public static void Clear() => _tables.Clear();
    }
}
