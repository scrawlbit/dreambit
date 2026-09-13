using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Animation;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Anima um rig de <see cref="Bone"/> por keyframes de pose. Fica no objeto raiz do
    /// personagem; os ossos são o próprio objeto e seus descendentes que tenham um Bone,
    /// identificados pelo nome. Cada keyframe guarda a pose local de cada osso num instante;
    /// no play (ou ao arrastar o playhead no editor) interpola entre keyframes e aplica.
    /// Osso = transform (cutout), sem deformação de malha.
    /// </summary>
    public sealed class SkeletonAnimator : SceneComponent
    {
        private readonly List<PoseKeyframe> _keyframes = new(); // sempre ordenado por Time
        private readonly List<(float Time, string Name)> _events = new(); // eventos por tempo
        private float _duration = 1f;
        private bool _loop = true;
        private float _time;
        private bool _playing;
        private Scrawlbit.EasingMode _easing = Scrawlbit.EasingMode.Linear;

        public override string DisplayName => "Skeleton Animator";

        /// <summary>Curva de suavização entre keyframes.</summary>
        public Scrawlbit.EasingMode Easing
        {
            get => _easing;
            set => Set(ref _easing, value);
        }

        /// <summary>Disparado quando a animação cruza um evento no tempo (nome do evento).</summary>
        public event System.Action<string>? AnimationEvent;

        public System.Collections.Generic.IEnumerable<(float Time, string Name)> Events => _events;

        public void SetEvents(System.Collections.Generic.IEnumerable<(float Time, string Name)> events)
        {
            _events.Clear();
            foreach (var e in events)
                if (e.Time >= 0f && !string.IsNullOrWhiteSpace(e.Name))
                    _events.Add((e.Time, e.Name.Trim()));
            _events.Sort((a, b) => a.Time.CompareTo(b.Time));
            OnPropertyChanged(nameof(Events));
        }

        public float Duration
        {
            get => _duration;
            set => Set(ref _duration, Math.Max(0.01f, value));
        }

        public bool Loop
        {
            get => _loop;
            set => Set(ref _loop, value);
        }

        /// <summary>Tempo atual (segundos) — usado para preview/scrub no editor.</summary>
        public float Time
        {
            get => _time;
            private set => Set(ref _time, value);
        }

        public IReadOnlyList<PoseKeyframe> Keyframes => _keyframes;
        public int KeyframeCount => _keyframes.Count;

        /// <summary>Ossos do rig (este objeto + descendentes com Bone), indexados por nome.</summary>
        public Dictionary<string, GameObject> RigBones()
        {
            var map = new Dictionary<string, GameObject>();
            Collect(Owner);
            return map;

            void Collect(GameObject obj)
            {
                if (obj.Components.OfType<Bone>().Any())
                    map[obj.Name] = obj; // nomes de osso devem ser únicos no rig
                foreach (var child in obj.Children)
                    Collect(child);
            }
        }

        /// <summary>Captura a pose atual dos ossos como um keyframe no instante informado
        /// (substitui um keyframe existente no mesmo tempo). Retorna o keyframe.</summary>
        public PoseKeyframe CaptureKeyframe(float time)
        {
            time = Scrawlbit.Mathf.Clamp(time, 0f, _duration);
            var bones = new Dictionary<string, BonePose>();
            foreach (var (name, obj) in RigBones())
            {
                var t = obj.Transform;
                bones[name] = new BonePose(t.Position, t.Rotation, t.Scale);
            }

            var existing = _keyframes.FirstOrDefault(k => Math.Abs(k.Time - time) < 0.0001f);
            if (existing != null)
            {
                existing.Bones.Clear();
                foreach (var kv in bones) existing.Bones[kv.Key] = kv.Value;
                OnPropertyChanged(nameof(Keyframes));
                return existing;
            }

            var frame = new PoseKeyframe(time, bones);
            InsertSorted(frame);
            OnPropertyChanged(nameof(Keyframes));
            OnPropertyChanged(nameof(KeyframeCount));
            return frame;
        }

        public void AddKeyframe(PoseKeyframe frame)
        {
            InsertSorted(frame);
            OnPropertyChanged(nameof(Keyframes));
            OnPropertyChanged(nameof(KeyframeCount));
        }

        /// <summary>Remove o keyframe mais próximo do tempo informado (dentro da tolerância).</summary>
        public bool RemoveKeyframeNear(float time, float tolerance = 0.02f)
        {
            var frame = _keyframes
                .OrderBy(k => Math.Abs(k.Time - time))
                .FirstOrDefault(k => Math.Abs(k.Time - time) <= tolerance);
            if (frame == null)
                return false;

            _keyframes.Remove(frame);
            OnPropertyChanged(nameof(Keyframes));
            OnPropertyChanged(nameof(KeyframeCount));
            return true;
        }

        private void InsertSorted(PoseKeyframe frame)
        {
            int i = _keyframes.FindIndex(k => k.Time > frame.Time);
            if (i < 0) _keyframes.Add(frame);
            else _keyframes.Insert(i, frame);
        }

        /// <summary>Define o tempo e aplica a pose interpolada (preview/scrub no editor).</summary>
        public void SetTime(float time)
        {
            Time = Scrawlbit.Mathf.Clamp(time, 0f, _duration);
            Sample(Time);
        }

        /// <summary>Amostra a pose no tempo informado e aplica aos ossos do rig.</summary>
        public void Sample(float time)
        {
            if (_keyframes.Count == 0)
                return;

            var pose = ResolvePose(time);
            var rig = RigBones();
            foreach (var (name, bonePose) in pose)
            {
                if (!rig.TryGetValue(name, out var obj))
                    continue;
                var t = obj.Transform;
                t.Position = bonePose.Position;
                t.Rotation = bonePose.Rotation;
                t.Scale = bonePose.Scale;
            }
        }

        /// <summary>Pose (por osso) interpolada no tempo informado, sem aplicar.</summary>
        public Dictionary<string, BonePose> ResolvePose(float time)
        {
            if (_keyframes.Count == 0)
                return new Dictionary<string, BonePose>();

            if (time <= _keyframes[0].Time)
                return new Dictionary<string, BonePose>(_keyframes[0].Bones);

            if (time >= _keyframes[^1].Time)
                return new Dictionary<string, BonePose>(_keyframes[^1].Bones);

            // Encontra o par de keyframes que cerca o tempo.
            for (int i = 0; i < _keyframes.Count - 1; i++)
            {
                var a = _keyframes[i];
                var b = _keyframes[i + 1];
                if (time < a.Time || time > b.Time)
                    continue;

                float factor = Scrawlbit.Easing.Apply(_easing, Scrawlbit.Mathf.InverseLerp(a.Time, b.Time, time));

                var result = new Dictionary<string, BonePose>();
                foreach (var (name, poseA) in a.Bones)
                {
                    result[name] = b.Bones.TryGetValue(name, out var poseB)
                        ? BonePose.Lerp(poseA, poseB, factor)
                        : poseA;
                }
                // ossos que só existem no keyframe B
                foreach (var (name, poseB) in b.Bones)
                    if (!result.ContainsKey(name))
                        result[name] = poseB;

                return result;
            }

            return new Dictionary<string, BonePose>(_keyframes[^1].Bones);
        }

        protected internal override void OnPlayStarted()
        {
            _time = 0f;
            _playing = _keyframes.Count > 0;
            if (_playing)
                Sample(0f);
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (!_playing || _keyframes.Count == 0)
                return;

            float prev = _time;
            _time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_time > _duration)
            {
                if (_loop)
                {
                    FireEventsInRange(prev, _duration); // fim da volta
                    _time %= _duration;
                    FireEventsInRange(0f, _time);       // início da nova volta
                }
                else
                {
                    _time = _duration;
                    _playing = false;
                    FireEventsInRange(prev, _duration);
                }
            }
            else
            {
                FireEventsInRange(prev, _time);
            }

            Sample(_time);
        }

        private void FireEventsInRange(float fromExclusive, float toInclusive)
        {
            foreach (var (time, name) in _events)
            {
                if (time > fromExclusive && time <= toInclusive)
                {
                    AnimationEvent?.Invoke(name);
                    Owner?.Scene?.Send(name, Owner); // encaminha ao barramento de eventos
                }
            }
        }
    }
}
