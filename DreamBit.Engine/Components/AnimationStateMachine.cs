using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>Condição de uma transição sobre um parâmetro.</summary>
    public enum AnimCondition { BoolTrue, BoolFalse, Trigger }

    /// <summary>Um estado da máquina: nome + clipe de animação que ele toca.</summary>
    public sealed class AnimStateDef
    {
        public string Name { get; set; } = "";
        public string Clip { get; set; } = "";
    }

    /// <summary>Uma transição: de um estado (ou "" = qualquer) para outro quando a condição vale.</summary>
    public sealed class AnimTransitionDef
    {
        public string From { get; set; } = "";  // "" = de qualquer estado
        public string To { get; set; } = "";
        public string Parameter { get; set; } = "";
        public AnimCondition Condition { get; set; } = AnimCondition.BoolTrue;
    }

    /// <summary>
    /// Máquina de estados de animação data-driven: estados (cada um toca um clipe) e transições
    /// disparadas por parâmetros (bool) e gatilhos (trigger). Dirige o <see cref="SpriteAnimator"/>
    /// ou o <see cref="SkeletonAnimator"/> do objeto, com crossfade. É o runtime que um editor
    /// visual (nó-e-fio) autora — equivale ao AnimationTree/Animator do Godot/Unity.
    /// </summary>
    public sealed class AnimationStateMachine : SceneComponent
    {
        private readonly List<AnimStateDef> _states = new();
        private readonly List<AnimTransitionDef> _transitions = new();
        private readonly Dictionary<string, bool> _bools = new();
        private readonly HashSet<string> _triggers = new();
        private string _defaultState = "";
        private float _blendTime = 0.12f;
        private string _current = "";

        public override string DisplayName => "Animation State Machine";

        public IReadOnlyList<AnimStateDef> States => _states;
        public IReadOnlyList<AnimTransitionDef> Transitions => _transitions;
        public string DefaultState { get => _defaultState; set => Set(ref _defaultState, value ?? string.Empty); }
        public float BlendTime { get => _blendTime; set => Set(ref _blendTime, System.Math.Max(0f, value)); }

        /// <summary>Estado atual em execução.</summary>
        public string CurrentState => _current;

        public void SetStates(IEnumerable<AnimStateDef> states)
        {
            _states.Clear();
            if (states != null) _states.AddRange(states);
        }

        public void SetTransitions(IEnumerable<AnimTransitionDef> transitions)
        {
            _transitions.Clear();
            if (transitions != null) _transitions.AddRange(transitions);
        }

        public AnimationStateMachine AddState(string name, string clip)
        {
            _states.Add(new AnimStateDef { Name = name, Clip = clip });
            return this;
        }

        public AnimationStateMachine AddTransition(string from, string to, string parameter, AnimCondition condition)
        {
            _transitions.Add(new AnimTransitionDef { From = from, To = to, Parameter = parameter, Condition = condition });
            return this;
        }

        /// <summary>Define um parâmetro booleano (ex.: "grounded", "moving").</summary>
        public void SetBool(string name, bool value) { if (!string.IsNullOrEmpty(name)) _bools[name] = value; }
        public bool GetBool(string name) => _bools.TryGetValue(name, out var v) && v;

        /// <summary>Dispara um gatilho de uso único (consumido ao transicionar).</summary>
        public void SetTrigger(string name) { if (!string.IsNullOrEmpty(name)) _triggers.Add(name); }

        protected internal override void OnPlayStarted()
        {
            _triggers.Clear();
            _current = string.IsNullOrEmpty(_defaultState) && _states.Count > 0 ? _states[0].Name : _defaultState;
            PlayClipFor(_current, 0f);
        }

        protected internal override void Update(GameTime gameTime)
        {
            // Avalia transições: primeiro as específicas do estado atual, depois as de "qualquer".
            foreach (var t in _transitions)
            {
                if (t.From != _current && !string.IsNullOrEmpty(t.From))
                    continue;
                if (t.To == _current || !_states.Any(s => s.Name == t.To))
                    continue;
                if (Met(t))
                {
                    if (t.Condition == AnimCondition.Trigger)
                        _triggers.Remove(t.Parameter); // consome o gatilho
                    _current = t.To;
                    PlayClipFor(_current, _blendTime);
                    return;
                }
            }
        }

        private bool Met(AnimTransitionDef t) => t.Condition switch
        {
            AnimCondition.BoolTrue => GetBool(t.Parameter),
            AnimCondition.BoolFalse => !GetBool(t.Parameter),
            AnimCondition.Trigger => _triggers.Contains(t.Parameter),
            _ => false
        };

        private void PlayClipFor(string state, float blend)
        {
            var def = _states.FirstOrDefault(s => s.Name == state);
            if (def == null || string.IsNullOrEmpty(def.Clip))
                return;

            var sprite = Owner?.Components.OfType<SpriteAnimator>().FirstOrDefault();
            if (sprite != null) { sprite.Play(def.Clip, blend); return; }

            var skeleton = Owner?.Components.OfType<SkeletonAnimator>().FirstOrDefault();
            skeleton?.Play(def.Clip, blend);
        }
    }
}
