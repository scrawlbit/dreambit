using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DreamBit.Engine.Data
{
    /// <summary>
    /// Catálogos data-driven: carrega tabelas de dados (JSON) em tipos que o próprio jogo
    /// define nos scripts — inimigos, itens, fases — e as consulta por id. Os valores ficam em
    /// arquivos, separados do código. Ex.: <c>DataCatalog.LoadMap&lt;Inimigo&gt;("enemies.json")</c>
    /// e depois <c>DataCatalog.Get&lt;Inimigo&gt;("goblin")</c>.
    /// </summary>
    public static class DataCatalog
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // typeof(T) -> Dictionary<string, T> (guardado como objeto)
        private static readonly Dictionary<Type, object> _maps = new();

        /// <summary>Carrega um mapa id→registro de um JSON (objeto) e o guarda em cache por tipo.</summary>
        public static IReadOnlyDictionary<string, T> LoadMap<T>(string path)
        {
            var json = File.ReadAllText(path);
            return LoadMapFromJson<T>(json);
        }

        /// <summary>Igual a <see cref="LoadMap{T}(string)"/>, mas a partir de uma string JSON.</summary>
        public static IReadOnlyDictionary<string, T> LoadMapFromJson<T>(string json)
        {
            var map = JsonSerializer.Deserialize<Dictionary<string, T>>(json, Options)
                      ?? new Dictionary<string, T>();
            _maps[typeof(T)] = map;
            return map;
        }

        /// <summary>Carrega uma lista de registros de um JSON (array). Não indexa por id.</summary>
        public static List<T> LoadList<T>(string path)
            => JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), Options) ?? new List<T>();

        /// <summary>Registro por id do mapa carregado do tipo T (default se ausente).</summary>
        public static T? Get<T>(string id)
        {
            if (_maps.TryGetValue(typeof(T), out var boxed) &&
                boxed is Dictionary<string, T> map && map.TryGetValue(id, out var value))
                return value;
            return default;
        }

        /// <summary>True se existe um registro com esse id no mapa do tipo T.</summary>
        public static bool Has<T>(string id)
            => _maps.TryGetValue(typeof(T), out var boxed) &&
               boxed is Dictionary<string, T> map && map.ContainsKey(id);

        /// <summary>Todos os registros carregados do tipo T.</summary>
        public static IEnumerable<T> All<T>()
            => _maps.TryGetValue(typeof(T), out var boxed) && boxed is Dictionary<string, T> map
                ? map.Values
                : System.Linq.Enumerable.Empty<T>();

        /// <summary>Esvazia os catálogos em cache.</summary>
        public static void Clear() => _maps.Clear();
    }
}
