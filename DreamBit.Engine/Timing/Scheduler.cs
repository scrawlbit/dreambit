using System;
using System.Collections.Generic;

namespace DreamBit.Engine.Timing
{
    /// <summary>
    /// Agenda callbacks por tempo — o equivalente prático a coroutines para os scripts:
    /// <c>Scheduler.After(2f, () => porta.Open())</c> ou <c>Scheduler.Every(0.5f, Spawn)</c>.
    /// A cena avança o relógio a cada frame (<see cref="Tick"/>) e limpa tudo ao (re)iniciar
    /// o play, então callbacks não vazam entre execuções.
    /// </summary>
    public static class Scheduler
    {
        private sealed class Entry
        {
            public float Remaining;
            public float Interval;   // 0 = one-shot
            public Action Action = () => { };
            public bool Cancelled;
        }

        private static readonly List<Entry> _entries = new();
        private static readonly List<Entry> _pending = new();

        /// <summary>Executa <paramref name="action"/> uma vez após <paramref name="seconds"/>.</summary>
        public static object After(float seconds, Action action)
        {
            var e = new Entry { Remaining = Math.Max(0f, seconds), Interval = 0f, Action = action };
            _pending.Add(e);
            return e;
        }

        /// <summary>Executa <paramref name="action"/> repetidamente a cada <paramref name="seconds"/>.</summary>
        public static object Every(float seconds, Action action)
        {
            float s = Math.Max(0.0001f, seconds);
            var e = new Entry { Remaining = s, Interval = s, Action = action };
            _pending.Add(e);
            return e;
        }

        /// <summary>Cancela um agendamento pelo handle devolvido por After/Every.</summary>
        public static void Cancel(object handle)
        {
            if (handle is Entry e)
                e.Cancelled = true;
        }

        /// <summary>Remove todos os agendamentos (chamado ao iniciar o play).</summary>
        public static void Clear()
        {
            _entries.Clear();
            _pending.Clear();
        }

        /// <summary>Quantos agendamentos ativos (para inspeção/testes).</summary>
        public static int Count => _entries.Count + _pending.Count;

        /// <summary>Avança o relógio; dispara os que venceram. Chamado pela cena por frame.</summary>
        public static void Tick(float dt)
        {
            if (_pending.Count > 0)
            {
                _entries.AddRange(_pending);
                _pending.Clear();
            }

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var e = _entries[i];
                if (e.Cancelled)
                {
                    _entries.RemoveAt(i);
                    continue;
                }

                e.Remaining -= dt;
                if (e.Remaining > 0f)
                    continue;

                e.Action();

                if (e.Interval > 0f && !e.Cancelled)
                    e.Remaining += e.Interval; // reagenda mantendo o resto do overflow
                else
                    _entries.RemoveAt(i);
            }
        }
    }
}
