using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Animation;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Anima um rig de <see cref="Bone"/> por keyframes de pose, organizados em clipes
    /// nomeados (ex.: "idle", "walk"). Um clipe é o ativo por vez; a API de edição
    /// (duração/loop/easing/keyframes/eventos) opera sobre o clipe ativo. No play (ou ao
    /// arrastar o playhead) interpola entre keyframes e aplica. Osso = transform (cutout).
    /// </summary>
    public sealed class SkeletonAnimator : SceneComponent
    {
        private readonly List<PoseClip> _clips = new();
        private PoseClip _current;
        private float _time;
        private bool _playing;

        // Crossfade entre clipes: pose de partida + duração/tempo do blend.
        private Dictionary<string, BonePose>? _blendFrom;
        private float _blendTime;
        private float _blendElapsed;

        public SkeletonAnimator()
        {
            _current = new PoseClip("default");
            _clips.Add(_current);
        }

        public override string DisplayName => "Skeleton Animator";

        // ---- clipes ----

        public IReadOnlyList<PoseClip> Clips => _clips;
        public IEnumerable<string> ClipNames => _clips.Select(c => c.Name);
        public PoseClip CurrentClip => _current;

        public string CurrentClipName
        {
            get => _current.Name;
            set
            {
                var clip = _clips.FirstOrDefault(c => c.Name == value);
                if (clip == null || clip == _current)
                    return;
                _current = clip;
                _time = 0f;
                OnPropertyChanged(nameof(CurrentClipName));
                RaiseClipChanged();
            }
        }

        /// <summary>Adiciona (ou retorna, se já existe) um clipe e o torna ativo.</summary>
        public PoseClip AddClip(string name)
        {
            name = string.IsNullOrWhiteSpace(name) ? "clip" : name.Trim();
            var existing = _clips.FirstOrDefault(c => c.Name == name);
            if (existing != null)
            {
                _current = existing;
            }
            else
            {
                var clip = new PoseClip(name);
                _clips.Add(clip);
                _current = clip;
                OnPropertyChanged(nameof(Clips));
                OnPropertyChanged(nameof(ClipNames));
            }
            _time = 0f;
            OnPropertyChanged(nameof(CurrentClipName));
            RaiseClipChanged();
            return _current;
        }

        /// <summary>Remove um clipe (não remove o último). Se remover o ativo, ativa outro.</summary>
        public bool RemoveClip(string name)
        {
            if (_clips.Count <= 1)
                return false;

            var clip = _clips.FirstOrDefault(c => c.Name == name);
            if (clip == null)
                return false;

            _clips.Remove(clip);
            if (_current == clip)
                _current = _clips[0];

            _time = 0f;
            OnPropertyChanged(nameof(Clips));
            OnPropertyChanged(nameof(ClipNames));
            OnPropertyChanged(nameof(CurrentClipName));
            RaiseClipChanged();
            return true;
        }

        /// <summary>Toca um clipe nomeado do início (runtime). Com <paramref name="blendTime"/> &gt; 0,
        /// faz crossfade suave da pose atual para o novo clipe (transição sem corte).</summary>
        public void Play(string name, float blendTime = 0f)
        {
            var clip = _clips.FirstOrDefault(c => c.Name == name);
            if (clip == null || clip == _current && _blendTime <= 0f && _time == 0f)
                return;

            if (blendTime > 0f && _current.Keyframes.Count > 0)
            {
                _blendFrom = ResolvePose(_time); // pose de onde estamos saindo
                _blendTime = blendTime;
                _blendElapsed = 0f;
            }
            else
            {
                _blendFrom = null;
                _blendTime = 0f;
            }

            _current = clip;
            _time = 0f;
            _playing = clip.Keyframes.Count > 0;
            OnPropertyChanged(nameof(CurrentClipName));
            RaiseClipChanged();
        }

        /// <summary>True enquanto uma transição (crossfade) entre clipes está em andamento.</summary>
        public bool IsBlending => _blendFrom != null && _blendElapsed < _blendTime;

        /// <summary>Disparado quando o clipe ativo muda ou seu conteúdo (para a UI atualizar).</summary>
        public event Action? ClipChanged;
        private void RaiseClipChanged()
        {
            ClipChanged?.Invoke();
            OnPropertyChanged(nameof(Keyframes));
            OnPropertyChanged(nameof(KeyframeCount));
            OnPropertyChanged(nameof(Events));
        }

        // ---- propriedades do clipe ativo ----

        public float Duration
        {
            get => _current.Duration;
            set { _current.Duration = Math.Max(0.01f, value); OnPropertyChanged(); }
        }

        public bool Loop
        {
            get => _current.Loop;
            set { _current.Loop = value; OnPropertyChanged(); }
        }

        public Scrawlbit.EasingMode Easing
        {
            get => _current.Easing;
            set { _current.Easing = value; OnPropertyChanged(); }
        }

        /// <summary>Tempo atual (segundos) — usado para preview/scrub no editor.</summary>
        public float Time
        {
            get => _time;
            private set => Set(ref _time, value);
        }

        public IReadOnlyList<PoseKeyframe> Keyframes => _current.Keyframes;
        public int KeyframeCount => _current.Keyframes.Count;

        /// <summary>Disparado quando a animação cruza um evento no tempo (nome do evento).</summary>
        public event Action<string>? AnimationEvent;

        public IEnumerable<(float Time, string Name)> Events => _current.Events;

        public void SetEvents(IEnumerable<(float Time, string Name)> events)
        {
            _current.Events.Clear();
            foreach (var e in events)
                if (e.Time >= 0f && !string.IsNullOrWhiteSpace(e.Name))
                    _current.Events.Add((e.Time, e.Name.Trim()));
            _current.Events.Sort((a, b) => a.Time.CompareTo(b.Time));
            OnPropertyChanged(nameof(Events));
        }

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

        /// <summary>Captura a pose atual dos ossos como um keyframe no clipe ativo.</summary>
        public PoseKeyframe CaptureKeyframe(float time)
        {
            time = Scrawlbit.Mathf.Clamp(time, 0f, _current.Duration);
            var bones = new Dictionary<string, BonePose>();
            foreach (var (name, obj) in RigBones())
            {
                var t = obj.Transform;
                bones[name] = new BonePose(t.Position, t.Rotation, t.Scale);
            }

            var existing = _current.Keyframes.FirstOrDefault(k => Math.Abs(k.Time - time) < 0.0001f);
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
            var frame = _current.Keyframes
                .OrderBy(k => Math.Abs(k.Time - time))
                .FirstOrDefault(k => Math.Abs(k.Time - time) <= tolerance);
            if (frame == null)
                return false;

            _current.Keyframes.Remove(frame);
            OnPropertyChanged(nameof(Keyframes));
            OnPropertyChanged(nameof(KeyframeCount));
            return true;
        }

        private void InsertSorted(PoseKeyframe frame)
        {
            int i = _current.Keyframes.FindIndex(k => k.Time > frame.Time);
            if (i < 0) _current.Keyframes.Add(frame);
            else _current.Keyframes.Insert(i, frame);
        }

        /// <summary>Define o tempo e aplica a pose interpolada (preview/scrub no editor).</summary>
        public void SetTime(float time)
        {
            Time = Scrawlbit.Mathf.Clamp(time, 0f, _current.Duration);
            Sample(Time);
        }

        /// <summary>Amostra a pose no tempo informado e aplica aos ossos do rig (mistura com a
        /// pose de partida se houver um crossfade em andamento).</summary>
        public void Sample(float time)
        {
            if (_current.Keyframes.Count == 0)
                return;

            var pose = ResolvePose(time);
            if (_blendFrom != null && _blendElapsed < _blendTime)
            {
                float f = _blendTime > 0f ? _blendElapsed / _blendTime : 1f;
                pose = BlendPoses(_blendFrom, pose, f);
            }

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

        private static Dictionary<string, BonePose> BlendPoses(
            Dictionary<string, BonePose> from, Dictionary<string, BonePose> to, float factor)
        {
            var result = new Dictionary<string, BonePose>();
            foreach (var (name, poseTo) in to)
                result[name] = from.TryGetValue(name, out var poseFrom)
                    ? BonePose.Lerp(poseFrom, poseTo, factor)
                    : poseTo;
            return result;
        }

        /// <summary>Pose (por osso) interpolada no tempo informado, sem aplicar.</summary>
        public Dictionary<string, BonePose> ResolvePose(float time)
        {
            var keyframes = _current.Keyframes;
            if (keyframes.Count == 0)
                return new Dictionary<string, BonePose>();

            if (time <= keyframes[0].Time)
                return new Dictionary<string, BonePose>(keyframes[0].Bones);

            if (time >= keyframes[^1].Time)
                return new Dictionary<string, BonePose>(keyframes[^1].Bones);

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                var a = keyframes[i];
                var b = keyframes[i + 1];
                if (time < a.Time || time > b.Time)
                    continue;

                float factor = Scrawlbit.Easing.Apply(_current.Easing, Scrawlbit.Mathf.InverseLerp(a.Time, b.Time, time));

                var result = new Dictionary<string, BonePose>();
                foreach (var (name, poseA) in a.Bones)
                {
                    result[name] = b.Bones.TryGetValue(name, out var poseB)
                        ? BonePose.Lerp(poseA, poseB, factor)
                        : poseA;
                }
                foreach (var (name, poseB) in b.Bones)
                    if (!result.ContainsKey(name))
                        result[name] = poseB;

                return result;
            }

            return new Dictionary<string, BonePose>(keyframes[^1].Bones);
        }

        protected internal override void OnPlayStarted()
        {
            _time = 0f;
            _playing = _current.Keyframes.Count > 0;
            if (_playing)
                Sample(0f);
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (!_playing || _current.Keyframes.Count == 0)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_blendFrom != null)
            {
                _blendElapsed += dt;
                if (_blendElapsed >= _blendTime)
                    _blendFrom = null; // transição concluída
            }

            float prev = _time;
            _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float duration = _current.Duration;

            if (_time > duration)
            {
                if (_current.Loop)
                {
                    FireEventsInRange(prev, duration);
                    _time %= duration;
                    FireEventsInRange(0f, _time);
                }
                else
                {
                    _time = duration;
                    _playing = false;
                    FireEventsInRange(prev, duration);
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
            foreach (var (time, name) in _current.Events)
            {
                if (time > fromExclusive && time <= toInclusive)
                {
                    AnimationEvent?.Invoke(name);
                    Owner?.Scene?.Send(name, Owner);
                }
            }
        }
    }
}
