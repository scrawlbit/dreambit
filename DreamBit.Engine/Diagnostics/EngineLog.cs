using System;
using System.Collections.Generic;

namespace DreamBit.Engine.Diagnostics
{
    public enum LogLevel { Info, Warning, Error }

    /// <summary>Uma entrada do console (nível, mensagem e horário).</summary>
    public readonly record struct LogEntry(LogLevel Level, string Message, DateTime Time)
    {
        public override string ToString() => $"{Time:HH:mm:ss} [{Level}] {Message}";
    }

    /// <summary>
    /// Canal de log compartilhado do motor. Componentes e scripts do usuário escrevem
    /// aqui (ex.: <c>EngineLog.Info("...")</c>) e o editor exibe num console. Mantém um
    /// buffer limitado; dispara <see cref="Logged"/> a cada entrada e <see cref="Cleared"/>
    /// ao limpar. Sem dependência de UI — o editor marshala para a thread dele.
    /// </summary>
    public static class EngineLog
    {
        private const int MaxEntries = 500;
        private static readonly List<LogEntry> _entries = new();
        private static readonly object _gate = new();

        /// <summary>Disparado quando uma nova entrada é registrada.</summary>
        public static event Action<LogEntry>? Logged;

        /// <summary>Disparado quando o log é limpo.</summary>
        public static event Action? Cleared;

        public static IReadOnlyList<LogEntry> Entries
        {
            get { lock (_gate) return _entries.ToArray(); }
        }

        public static void Info(string message) => Write(LogLevel.Info, message);
        public static void Warn(string message) => Write(LogLevel.Warning, message);
        public static void Error(string message) => Write(LogLevel.Error, message);

        public static void Write(LogLevel level, string message)
        {
            var entry = new LogEntry(level, message ?? string.Empty, DateTime.Now);
            lock (_gate)
            {
                _entries.Add(entry);
                if (_entries.Count > MaxEntries)
                    _entries.RemoveRange(0, _entries.Count - MaxEntries);
            }
            Logged?.Invoke(entry);
        }

        public static void Clear()
        {
            lock (_gate)
                _entries.Clear();
            Cleared?.Invoke();
        }
    }
}
