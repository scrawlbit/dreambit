using System;

namespace Scrawlbit
{
    /// <summary>Helpers de enum (listar valores, parse tolerante).</summary>
    public static class EnumHelper
    {
        /// <summary>Todos os valores de um enum.</summary>
        public static T[] Values<T>() where T : struct, Enum => (T[])Enum.GetValues(typeof(T));

        /// <summary>Faz parse ignorando caixa; retorna o padrão se não reconhecer.</summary>
        public static T ParseOrDefault<T>(string? text, T fallback = default) where T : struct, Enum
            => Enum.TryParse<T>(text, ignoreCase: true, out var value) ? value : fallback;
    }
}
