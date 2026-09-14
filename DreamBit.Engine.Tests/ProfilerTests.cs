using System.Linq;
using DreamBit.Engine.Diagnostics;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class ProfilerTests
    {
        [Fact]
        public void Profiler_MedeMediaMovelEContadores()
        {
            Profiler.Reset();
            for (int i = 0; i < 30; i++)
            {
                Profiler.Record("update", 4.0);
                Profiler.EndFrame();
            }
            Assert.Equal(4.0, Profiler.AverageMs("update"), 0.2);

            Profiler.SetCounter("obj", 42);
            var report = Profiler.Report().ToList();
            Assert.True(report.Any(l => l.StartsWith("update")), "relata a seção");
            Assert.True(report.Any(l => l.Contains("obj") && l.Contains("42")), "relata o contador");
        }

        [Fact]
        public void Profiler_Scope_Cronometra()
        {
            Profiler.Reset();
            using (Profiler.Scope("bloco"))
                System.Threading.Thread.Sleep(2);
            Profiler.EndFrame();
            Assert.True(Profiler.AverageMs("bloco") > 0.5, "o escopo mediu algum tempo");
        }
    }
}
