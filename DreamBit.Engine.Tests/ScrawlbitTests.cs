using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scrawlbit;
using Scrawlbit.Collections;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ScrawlbitTests
    {
        [TestMethod]
        public void Mathf_ClampLerpInverseLerp()
        {
            Assert.AreEqual(5f, Mathf.Clamp(10f, 0f, 5f), 0.0001f);
            Assert.AreEqual(0f, Mathf.Clamp(-3f, 0f, 5f), 0.0001f);
            Assert.AreEqual(15f, Mathf.Lerp(10f, 20f, 0.5f), 0.0001f);
            Assert.AreEqual(0.5f, Mathf.InverseLerp(10f, 20f, 15f), 0.0001f);
            Assert.AreEqual(0f, Mathf.InverseLerp(10f, 10f, 15f), 0.0001f, "a == b não deve dividir por zero");
        }

        [TestMethod]
        public void Mathf_MoveTowards_E_Approximately()
        {
            Assert.AreEqual(5f, Mathf.MoveTowards(0f, 5f, 100f), 0.0001f, "não passa do alvo");
            Assert.AreEqual(2f, Mathf.MoveTowards(0f, 5f, 2f), 0.0001f);
            Assert.IsTrue(Mathf.Approximately(0.1f + 0.2f, 0.3f));
            Assert.IsFalse(Mathf.Approximately(0.1f, 0.2f));
        }

        [TestMethod]
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

            Assert.AreEqual(4, col.Count);
            Assert.AreEqual(1, resets, "AddRange deve disparar um unico Reset");
        }

        [TestMethod]
        public void FuncEqualityComparer_ByKey()
        {
            var cmp = FuncEqualityComparer<(int Id, string Nome)>.ByKey(x => x.Id);
            var lista = new[] { (1, "a"), (1, "b"), (2, "c") };
            var distintos = lista.Distinct(cmp).ToList();
            Assert.AreEqual(2, distintos.Count, "itens com a mesma chave sao iguais");
        }

        [TestMethod]
        public void EnumHelper_Values()
        {
            var valores = EnumHelper.Values<System.DayOfWeek>();
            Assert.AreEqual(7, valores.Length);
            Assert.AreEqual(System.DayOfWeek.Monday, EnumHelper.ParseOrDefault("monday", System.DayOfWeek.Sunday));
            Assert.AreEqual(System.DayOfWeek.Sunday, EnumHelper.ParseOrDefault("xxx", System.DayOfWeek.Sunday));
        }
    }
}
