using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DreamBit.Engine.Tests
{
    /// <summary>
    /// Ajudantes finos sobre <see cref="Xunit.Assert"/> para asserções de coleção/string,
    /// preservando a legibilidade dos testes na migração para o xUnit.
    /// </summary>
    internal static class CollectionAssert
    {
        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
            => Assert.Equal(expected, actual);

        public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var e = expected.ToList();
            var a = actual.ToList();
            Assert.Equal(e.Count, a.Count);
            foreach (var item in e) Assert.Contains(item, a);
            foreach (var item in a) Assert.Contains(item, e);
        }

        public static void Contains<T>(IEnumerable<T> collection, T item)
            => Assert.Contains(item, collection);

        public static void DoesNotContain<T>(IEnumerable<T> collection, T item)
            => Assert.DoesNotContain(item, collection);
    }

    internal static class StringAssert
    {
        public static void Contains(string value, string substring)
            => Assert.Contains(substring, value);
    }
}
