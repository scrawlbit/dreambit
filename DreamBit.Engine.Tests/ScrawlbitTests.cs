using System.Linq;
using Xunit;
using Scrawlbit;
using Scrawlbit.Collections;

namespace DreamBit.Engine.Tests
{
    public class ScrawlbitTests
    {
        [Fact]
        public void Mathf_ClampLerpInverseLerp()
        {
            Assert.Equal(5f, Mathf.Clamp(10f, 0f, 5f), 0.0001f);
            Assert.Equal(0f, Mathf.Clamp(-3f, 0f, 5f), 0.0001f);
            Assert.Equal(15f, Mathf.Lerp(10f, 20f, 0.5f), 0.0001f);
            Assert.Equal(0.5f, Mathf.InverseLerp(10f, 20f, 15f), 0.0001f);
            Assert.Equal(0f, Mathf.InverseLerp(10f, 10f, 15f), 0.0001f);
        }

        [Fact]
        public void Mathf_MoveTowards_E_Approximately()
        {
            Assert.Equal(5f, Mathf.MoveTowards(0f, 5f, 100f), 0.0001f);
            Assert.Equal(2f, Mathf.MoveTowards(0f, 5f, 2f), 0.0001f);
            Assert.True(Mathf.Approximately(0.1f + 0.2f, 0.3f));
            Assert.False(Mathf.Approximately(0.1f, 0.2f));
        }

        [Fact]
        public void ExtendedObservableCollection_AddRange_UmUnicoReset()
        {
            var col = new ExtendedObservableCollection<int>();
            int resets = 0;
            col.CollectionChanged += (_, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                    resets++;
            };

            col.AddRange(new[] { 1, 2, 3, 4 });

            Assert.Equal(4, col.Count);
            Assert.Equal(1, resets);
        }

        [Fact]
        public void FuncEqualityComparer_ByKey()
        {
            var cmp = FuncEqualityComparer<(int Id, string Nome)>.ByKey(x => x.Id);
            var lista = new[] { (1, "a"), (1, "b"), (2, "c") };
            var distintos = lista.Distinct(cmp).ToList();
            Assert.Equal(2, distintos.Count);
        }

        [Fact]
        public void EnumHelper_Values()
        {
            var valores = EnumHelper.Values<System.DayOfWeek>();
            Assert.Equal(7, valores.Length);
            Assert.Equal(System.DayOfWeek.Monday, EnumHelper.ParseOrDefault("monday", System.DayOfWeek.Sunday));
            Assert.Equal(System.DayOfWeek.Sunday, EnumHelper.ParseOrDefault("xxx", System.DayOfWeek.Sunday));
        }
    }
}
