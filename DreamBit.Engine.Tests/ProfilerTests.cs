using System.Linq;
using DreamBit.Engine.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ProfilerTests
    {
        [TestMethod]
        public void Profiler_MedeMediaMovelEContadores()
        {
            Profiler.Reset();
            for (int i = 0; i < 30; i++)
            {
                Profiler.Record("update", 4.0);
                Profiler.EndFrame();
            }
            Assert.AreEqual(4.0, Profiler.AverageMs("update"), 0.2, "média converge para o tempo registrado");

            Profiler.SetCounter("obj", 42);
            var report = Profiler.Report().ToList();
            Assert.IsTrue(report.Any(l => l.StartsWith("update")), "relata a seção");
            Assert.IsTrue(report.Any(l => l.Contains("obj") && l.Contains("42")), "relata o contador");
        }

        [TestMethod]
        public void Profiler_Scope_Cronometra()
        {
            Profiler.Reset();
            using (Profiler.Scope("bloco"))
                System.Threading.Thread.Sleep(2);
            Profiler.EndFrame();
            Assert.IsTrue(Profiler.AverageMs("bloco") > 0.5, "o escopo mediu algum tempo");
        }
    }
}
