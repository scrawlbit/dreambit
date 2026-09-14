using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Diagnostics;
using DreamBit.Engine.Elements;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class EngineLogTests
    {
        public EngineLogTests() => EngineLog.Clear();

        [Fact]
        public void Write_RegistraEntradaComNivel_EDisparaEvento()
        {
            var recebidas = new List<LogEntry>();
            void Handler(LogEntry e) => recebidas.Add(e);
            EngineLog.Logged += Handler;
            try
            {
                EngineLog.Info("oi");
                EngineLog.Warn("cuidado");
                EngineLog.Error("falhou");
            }
            finally { EngineLog.Logged -= Handler; }

            Assert.Equal(3, recebidas.Count);
            Assert.Equal(LogLevel.Info, recebidas[0].Level);
            Assert.Equal(LogLevel.Warning, recebidas[1].Level);
            Assert.Equal(LogLevel.Error, recebidas[2].Level);
            Assert.Equal("falhou", recebidas[2].Message);
        }

        [Fact]
        public void Clear_EsvaziaBuffer_EDisparaCleared()
        {
            EngineLog.Info("a");
            EngineLog.Info("b");
            Assert.Equal(2, EngineLog.Entries.Count);

            bool cleared = false;
            void Handler() => cleared = true;
            EngineLog.Cleared += Handler;
            try { EngineLog.Clear(); }
            finally { EngineLog.Cleared -= Handler; }

            Assert.Empty(EngineLog.Entries);
            Assert.True(cleared);
        }

        [Fact]
        public void Buffer_LimitaEmQuinhentasEntradas()
        {
            for (int i = 0; i < 600; i++)
                EngineLog.Info($"m{i}");

            Assert.Equal(500, EngineLog.Entries.Count);
            // As mais antigas saem; a última permanece.
            Assert.Equal("m599", EngineLog.Entries.Last().Message);
        }

        [Fact]
        public void ScriptComponent_ErroDeCompilacao_VaiParaOConsole()
        {
            var obj = new GameObject("Herói");
            var script = new ScriptComponent { Source = "isso nao compila {" };
            obj.AddComponent(script);

            script.Compile();

            Assert.True(EngineLog.Entries.Any(e => e.Level == LogLevel.Error && e.Message.Contains("Herói")), "erro de compilação do script deveria ter sido logado com o nome do objeto");
        }

        [Fact]
        public void ScriptComponent_ExcecaoEmRuntime_LogaUmaVez()
        {
            var scene = new Scene();
            var obj = new GameObject("Bug");
            obj.AddComponent(new ScriptComponent
            {
                Source =
                    "public class S : IGameScript {" +
                    "  public void Update(GameObject self, float dt) {" +
                    "    throw new System.Exception(\"boom\");" +
                    "  }" +
                    "}"
            });
            scene.Add(obj);
            scene.StartPlay(); // compila o script

            var gt = new GameTime(System.TimeSpan.Zero, System.TimeSpan.FromMilliseconds(16));
            // Vários frames: só deve logar uma vez (não inunda o console).
            for (int i = 0; i < 5; i++)
                scene.Update(gt);

            int erros = EngineLog.Entries.Count(e => e.Level == LogLevel.Error && e.Message.Contains("boom"));
            Assert.Equal(1, erros);
        }
    }
}
