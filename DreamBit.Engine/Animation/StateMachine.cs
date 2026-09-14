using System;
using System.Collections.Generic;

namespace DreamBit.Engine.Animation
{
    /// <summary>
    /// Máquina de estados genérica (estados nomeados + transições por condição), para scripts
    /// controlarem animação/IA de forma flexível, além do <c>AnimatorController</c> fixo de
    /// idle/walk/jump. Dirigida por código: cada estado tem callbacks (enter/update/exit) e cada
    /// transição uma condição <see cref="Func{Boolean}"/>.
    /// </summary>
    public sealed class StateMachine
    {
        private sealed class State
        {
            public string Name = "";
            public Action? OnEnter;
            public Action<float>? OnUpdate;
            public Action? OnExit;
            public readonly List<(Func<bool> Cond, string To)> Transitions = new();
        }

        private readonly Dictionary<string, State> _states = new(StringComparer.Ordinal);
        private readonly List<(Func<bool> Cond, string To)> _anyTransitions = new();
        private State? _current;

        /// <summary>Estado atual (vazio antes de <see cref="Start"/>).</summary>
        public string Current => _current?.Name ?? string.Empty;

        /// <summary>Disparado a cada troca de estado (para logs/animação): (de, para).</summary>
        public event Action<string, string>? Changed;

        /// <summary>Registra um estado com callbacks opcionais. Encadeável.</summary>
        public StateMachine AddState(string name, Action? onEnter = null, Action<float>? onUpdate = null, Action? onExit = null)
        {
            _states[name] = new State { Name = name, OnEnter = onEnter, OnUpdate = onUpdate, OnExit = onExit };
            return this;
        }

        /// <summary>Transição de <paramref name="from"/> para <paramref name="to"/> quando a condição for verdadeira.</summary>
        public StateMachine AddTransition(string from, string to, Func<bool> condition)
        {
            if (_states.TryGetValue(from, out var s))
                s.Transitions.Add((condition, to));
            return this;
        }

        /// <summary>Transição a partir de qualquer estado (avaliada antes das específicas).</summary>
        public StateMachine AddAnyTransition(string to, Func<bool> condition)
        {
            _anyTransitions.Add((condition, to));
            return this;
        }

        /// <summary>Define o estado inicial (dispara o OnEnter dele).</summary>
        public void Start(string state)
        {
            if (!_states.TryGetValue(state, out var s))
                return;
            _current = s;
            s.OnEnter?.Invoke();
        }

        /// <summary>Força a troca para um estado (respeitando exit/enter).</summary>
        public void Go(string state)
        {
            if (!_states.TryGetValue(state, out var next) || ReferenceEquals(next, _current))
                return;
            var from = _current?.Name ?? string.Empty;
            _current?.OnExit?.Invoke();
            _current = next;
            next.OnEnter?.Invoke();
            Changed?.Invoke(from, next.Name);
        }

        /// <summary>Avança um frame: avalia transições (any + do estado) e roda o update do estado.</summary>
        public void Update(float dt)
        {
            if (_current == null)
                return;

            foreach (var (cond, to) in _anyTransitions)
                if (!string.Equals(to, _current.Name, StringComparison.Ordinal) && cond())
                {
                    Go(to);
                    break;
                }

            foreach (var (cond, to) in _current.Transitions)
                if (cond())
                {
                    Go(to);
                    break;
                }

            _current.OnUpdate?.Invoke(dt);
        }
    }
}
