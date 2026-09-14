using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DreamBit.Engine.Diagnostics
{
    /// <summary>
    /// Profiler leve: mede o tempo (ms) de seções nomeadas por frame (com média móvel) e guarda
    /// contadores (objetos, partículas, draw calls). O host mostra no overlay de debug (F3).
    /// Equivale ao monitor de performance do Godot. Use <see cref="Begin"/>/<see cref="End"/> ou
    /// <see cref="Scope"/> em volta de um bloco; chame <see cref="EndFrame"/> uma vez por frame.
    /// </summary>
    public static class Profiler
    {
        private sealed class Section { public double FrameMs; public double AvgMs; public long StartTicks; }

        private static readonly Dictionary<string, Section> _sections = new();
        private static readonly Dictionary<string, double> _counters = new();
        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        /// <summary>Liga/desliga a coleta (o overlay do host respeita isto).</summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>Começa a medir uma seção.</summary>
        public static void Begin(string name)
        {
            if (!Enabled) return;
            var s = Get(name);
            s.StartTicks = _sw.ElapsedTicks;
        }

        /// <summary>Termina a seção e soma o tempo ao acumulado do frame.</summary>
        public static void End(string name)
        {
            if (!Enabled) return;
            var s = Get(name);
            double ms = (_sw.ElapsedTicks - s.StartTicks) * 1000.0 / Stopwatch.Frequency;
            s.FrameMs += ms;
        }

        /// <summary>Escopo cronometrado: <c>using (Profiler.Scope("fisica")) { ... }</c>.</summary>
        public static IDisposable Scope(string name) => new Timing(name);

        /// <summary>Registra um tempo (ms) diretamente (para hosts/teste).</summary>
        public static void Record(string name, double ms)
        {
            if (!Enabled) return;
            Get(name).FrameMs += ms;
        }

        /// <summary>Define um contador nomeado (objetos, partículas, etc.).</summary>
        public static void SetCounter(string name, double value) => _counters[name] = value;

        /// <summary>Fecha o frame: consolida as médias móveis e zera o acúmulo do frame.</summary>
        public static void EndFrame()
        {
            foreach (var s in _sections.Values)
            {
                s.AvgMs = s.AvgMs <= 0 ? s.FrameMs : s.AvgMs * 0.9 + s.FrameMs * 0.1;
                s.FrameMs = 0;
            }
        }

        /// <summary>Tempo médio (ms) de uma seção.</summary>
        public static double AverageMs(string name) => _sections.TryGetValue(name, out var s) ? s.AvgMs : 0;

        /// <summary>Linhas prontas para o overlay (seções em ms + contadores).</summary>
        public static IEnumerable<string> Report()
        {
            foreach (var kv in _sections.OrderBy(k => k.Key))
                yield return $"{kv.Key} {kv.Value.AvgMs:0.0}ms";
            foreach (var kv in _counters.OrderBy(k => k.Key))
                yield return $"{kv.Key} {kv.Value:0}";
        }

        /// <summary>Limpa tudo (troca de cena/teste).</summary>
        public static void Reset() { _sections.Clear(); _counters.Clear(); }

        private static Section Get(string name)
        {
            if (!_sections.TryGetValue(name, out var s))
                _sections[name] = s = new Section();
            return s;
        }

        private readonly struct Timing : IDisposable
        {
            private readonly string _name;
            public Timing(string name) { _name = name; Begin(name); }
            public void Dispose() => End(_name);
        }
    }
}
