using System;
using System.Collections.Generic;

namespace Scrawlbit
{
    /// <summary>Comparador de igualdade a partir de funções (ex.: comparar por uma chave).</summary>
    public sealed class FuncEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T?, T?, bool> _equals;
        private readonly Func<T, int> _hash;

        public FuncEqualityComparer(Func<T?, T?, bool> equals, Func<T, int>? hash = null)
        {
            _equals = equals ?? throw new ArgumentNullException(nameof(equals));
            _hash = hash ?? (x => x?.GetHashCode() ?? 0);
        }

        /// <summary>Compara por uma chave derivada (ex.: <c>ByKey(x =&gt; x.Id)</c>).</summary>
        public static FuncEqualityComparer<T> ByKey<TKey>(Func<T, TKey> keySelector)
            => new((a, b) =>
                {
                    if (a is null || b is null) return ReferenceEquals(a, b);
                    return EqualityComparer<TKey>.Default.Equals(keySelector(a), keySelector(b));
                },
                x => keySelector(x)?.GetHashCode() ?? 0);

        public bool Equals(T? x, T? y) => _equals(x, y);
        public int GetHashCode(T obj) => _hash(obj);
    }
}
