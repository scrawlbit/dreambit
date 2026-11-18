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
    /// <para>
    /// Além do transform, cada keyframe pode ter, por osso (<see cref="BoneKey"/>): curva própria
    /// (easing/step/bézier), troca de sprite, ordem de desenho, cor/alpha e visibilidade da
    /// parte pintada do osso. A "parte" de um osso é o SpriteRenderer do próprio osso ou, se
    /// ele não tiver, o do primeiro filho que não é osso. Valores fora das chaves voltam ao
    /// original da parte (o que estava antes de a animação mexer). Depois da pose, aplica as
    /// restrições <see cref="IkConstraint"/> do rig. Skins trocam todas as partes de uma vez.
    /// </para>
    /// </summary>
    public sealed class SkeletonAnimator : SceneComponent
    {
        private readonly List<PoseClip> _clips = new();
        private readonly List<Skin> _skins = new();
        private PoseClip _current;
        private float _time;
        private bool _playing;
        private bool _finished;
        private bool _atClipStart;
        private float _speed = 1f;
        private string _skin = string.Empty;

        // Crossfade entre clipes: pose de partida + duração/tempo do blend.
        private Dictionary<string, BonePose>? _blendFrom;
        private float _blendTime;
        private float _blendElapsed;

        // Fila de clipes a tocar quando o atual terminar.
        private readonly Queue<(string Clip, float Blend)> _queue = new();

        // Handlers por nome de evento (OnEvent).
        private readonly Dictionary<string, List<Action>> _eventHandlers = new(StringComparer.Ordinal);

        // Valores originais das partes enquanto a animação as sobrepõe (para restaurar).
        private readonly Dictionary<SpriteRenderer, PartOverride> _overrides = new();

        private sealed class PartOverride
        {
            public bool Sprite, Sort, Tint, Visible;
            public string? Texture;
            public Rectangle? Source;
            public int SortOrder;
            public Color TintColor;
            public bool Enabled;
            public bool Any => Sprite || Sort || Tint || Visible;
        }

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

        /// <summary>Clipe pelo nome, ou nulo.</summary>
        public PoseClip? GetClip(string name) => _clips.FirstOrDefault(c => c.Name == name);

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

        // ---- reprodução ----

        /// <summary>Toca um clipe nomeado do início (runtime). Com <paramref name="blendTime"/> &gt; 0,
        /// faz crossfade suave da pose atual para o novo clipe (transição sem corte). Limpa a fila.</summary>
        public void Play(string name, float blendTime = 0f)
        {
            var clip = _clips.FirstOrDefault(c => c.Name == name);
            if (clip == null || clip == _current && _blendTime <= 0f && _time == 0f && _playing)
                return;

            _queue.Clear();
            StartClip(clip, blendTime);
        }

        /// <summary>Toca o clipe; com <paramref name="restart"/> = false não reinicia se ele já é o
        /// clipe ativo e está tocando (útil para chamar todo frame a partir do estado do jogo).</summary>
        public void Play(string name, float blendTime, bool restart)
        {
            if (!restart && _current.Name == name && _playing)
                return;
            Play(name, blendTime);
        }

        private void StartClip(PoseClip clip, float blendTime)
        {
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
            _finished = false;
            _atClipStart = true;
            _playing = clip.Keyframes.Count > 0;
            OnPropertyChanged(nameof(CurrentClipName));
            RaiseClipChanged();
        }

        /// <summary>Enfileira um clipe para tocar quando o atual terminar (fim de um clipe sem loop,
        /// ou fim do ciclo atual de um clipe em loop). Se nada estiver tocando, toca já.</summary>
        public void Queue(string name, float blendTime = 0f)
        {
            if (GetClip(name) == null)
                return;
            if (!_playing)
            {
                Play(name, blendTime);
                return;
            }
            _queue.Enqueue((name, blendTime));
        }

        /// <summary>Descarta os clipes enfileirados.</summary>
        public void ClearQueue() => _queue.Clear();

        /// <summary>Clipes na fila (na ordem em que vão tocar).</summary>
        public IEnumerable<string> QueuedClips => _queue.Select(q => q.Clip);

        /// <summary>Velocidade de reprodução (1 = normal, 0.5 = câmera lenta, 0 = pausado).</summary>
        public float Speed
        {
            get => _speed;
            set => Set(ref _speed, Math.Max(0f, value));
        }

        /// <summary>True enquanto o clipe ativo está avançando no tempo.</summary>
        public bool IsPlaying => _playing;

        /// <summary>True quando um clipe sem loop chegou ao fim (e não há fila).</summary>
        public bool IsFinished => _finished;

        /// <summary>Pausa/retoma a reprodução sem perder o tempo atual.</summary>
        public void Pause() => _playing = false;

        public void Resume()
        {
            if (_current.Keyframes.Count > 0 && !_finished)
                _playing = true;
        }

        /// <summary>Tempo normalizado no clipe ativo (0 = início, 1 = fim). Definir aplica a pose.</summary>
        public float NormalizedTime
        {
            get => _current.Duration > 0f ? Scrawlbit.Mathf.Clamp01(_time / _current.Duration) : 0f;
            set => SetTime(Scrawlbit.Mathf.Clamp01(value) * _current.Duration);
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

        /// <summary>Disparado quando o clipe chega ao fim (a cada ciclo, em loop), com o nome do clipe.</summary>
        public event Action<string>? ClipCompleted;

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

        // ---- eventos ----

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

        /// <summary>Registra um handler para um evento nomeado (ex.: "passo", "golpe").</summary>
        public void OnEvent(string name, Action handler)
        {
            if (!_eventHandlers.TryGetValue(name, out var list))
                _eventHandlers[name] = list = new List<Action>();
            list.Add(handler);
        }

        /// <summary>Remove um handler registrado com <see cref="OnEvent"/>.</summary>
        public void RemoveEventHandler(string name, Action handler)
        {
            if (_eventHandlers.TryGetValue(name, out var list))
                list.Remove(handler);
        }

        // ---- rig ----

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

        /// <summary>Objeto do rig (este objeto ou descendente) com o nome informado, ou nulo.</summary>
        public GameObject? FindInRig(string name)
        {
            if (Owner == null || string.IsNullOrEmpty(name))
                return null;
            return Find(Owner);

            GameObject? Find(GameObject obj)
            {
                if (obj.Name == name)
                    return obj;
                foreach (var child in obj.Children)
                {
                    var found = Find(child);
                    if (found != null)
                        return found;
                }
                return null;
            }
        }

        /// <summary>Parte pintada (SpriteRenderer) de um objeto do rig: a do próprio objeto ou,
        /// se não tiver, a do primeiro filho que não é osso.</summary>
        public static SpriteRenderer? PartOf(GameObject obj)
        {
            var own = obj.Components.OfType<SpriteRenderer>().FirstOrDefault();
            if (own != null)
                return own;
            foreach (var child in obj.Children)
            {
                if (child.Components.OfType<Bone>().Any())
                    continue;
                var sprite = child.Components.OfType<SpriteRenderer>().FirstOrDefault();
                if (sprite != null)
                    return sprite;
            }
            return null;
        }

        /// <summary>Parte pintada pelo nome do osso/objeto no rig, ou nulo.</summary>
        public SpriteRenderer? FindPart(string name)
        {
            var obj = FindInRig(name);
            return obj != null ? PartOf(obj) : null;
        }

        // ---- keyframes ----

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
                // Recaptura só o transform: curva e extras da chave são preservados.
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

        /// <summary>Keyframe do clipe ativo no tempo informado (dentro da tolerância), ou nulo.</summary>
        public PoseKeyframe? KeyframeAt(float time, float tolerance = 0.02f)
            => _current.KeyframeAt(time, tolerance);

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

        // ---- amostragem ----

        /// <summary>Define o tempo e aplica a pose interpolada (preview/scrub no editor).</summary>
        public void SetTime(float time)
        {
            Time = Scrawlbit.Mathf.Clamp(time, 0f, _current.Duration);
            Sample(Time);
        }

        /// <summary>Amostra a pose no tempo informado e aplica aos ossos do rig (mistura com a
        /// pose de partida se houver um crossfade em andamento), depois aplica sprite/ordem/cor/
        /// visibilidade das partes e as restrições de IK.</summary>
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

            ApplyBoneKeys(time, rig);
            ApplyConstraints();
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

        /// <summary>Curva do segmento que começa na chave <paramref name="key"/> para o osso
        /// (curva do osso na chave, senão a do keyframe, senão o easing do clipe).</summary>
        public KeyCurve CurveFor(PoseKeyframe key, string bone)
        {
            if (key.BoneKeys.TryGetValue(bone, out var bk) && bk.Curve is { } own)
                return own;
            return key.Curve ?? KeyCurve.FromEasing(_current.Easing);
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

                float u = Scrawlbit.Mathf.InverseLerp(a.Time, b.Time, time);
                var keyCurve = a.Curve ?? KeyCurve.FromEasing(_current.Easing);

                var result = new Dictionary<string, BonePose>();
                foreach (var (name, poseA) in a.Bones)
                {
                    if (!b.Bones.TryGetValue(name, out var poseB))
                    {
                        result[name] = poseA;
                        continue;
                    }
                    var curve = a.BoneKeys.TryGetValue(name, out var bk) && bk.Curve is { } own ? own : keyCurve;
                    result[name] = BonePose.Lerp(poseA, poseB, curve.Evaluate(u));
                }
                foreach (var (name, poseB) in b.Bones)
                    if (!result.ContainsKey(name))
                        result[name] = poseB;

                return result;
            }

            return new Dictionary<string, BonePose>(keyframes[^1].Bones);
        }

        /// <summary>
        /// Sprite/ordem/cor/visibilidade resolvidos por osso no tempo informado, sem aplicar.
        /// Discretos: valor da última chave (≤ tempo) que o define. Cor: interpolada entre a
        /// chave anterior e a próxima que definem cor (com a curva da anterior). Antes da primeira
        /// chave que define um valor, ele fica nulo (= original da parte).
        /// </summary>
        public Dictionary<string, BoneKeySample> ResolveBoneKeys(float time) => ResolveBoneKeys(_current, time);

        private Dictionary<string, BoneKeySample> ResolveBoneKeys(PoseClip clip, float time)
        {
            var result = new Dictionary<string, BoneKeySample>();
            var keyframes = clip.Keyframes;
            if (keyframes.Count == 0)
                return result;

            var bones = new HashSet<string>();
            foreach (var k in keyframes)
                foreach (var name in k.BoneKeys.Keys)
                    bones.Add(name);

            const float eps = 0.00001f;
            foreach (var bone in bones)
            {
                SpriteRef? sprite = null;
                int? sort = null;
                bool? visible = null;
                Color? tintPrev = null;
                PoseKeyframe? tintPrevKey = null;
                Color? tintNext = null;
                PoseKeyframe? tintNextKey = null;

                foreach (var k in keyframes)
                {
                    if (!k.BoneKeys.TryGetValue(bone, out var bk))
                        continue;
                    if (k.Time <= time + eps)
                    {
                        if (bk.Sprite is { } s) sprite = s;
                        if (bk.SortOrder is { } o) sort = o;
                        if (bk.Visible is { } v) visible = v;
                        if (bk.Tint is { } c) { tintPrev = c; tintPrevKey = k; }
                    }
                    else if (tintNext == null && bk.Tint is { } c)
                    {
                        tintNext = c;
                        tintNextKey = k;
                    }
                }

                Color? tint = tintPrev;
                if (tintPrev is { } from && tintNext is { } to && tintPrevKey != null && tintNextKey != null)
                {
                    float u = Scrawlbit.Mathf.InverseLerp(tintPrevKey.Time, tintNextKey.Time, time);
                    tint = Color.Lerp(from, to, CurveFor(tintPrevKey, bone).Evaluate(u));
                }

                var sample = new BoneKeySample(sprite, sort, tint, visible);
                if (!sample.IsEmpty)
                    result[bone] = sample;
            }
            return result;
        }

        private void ApplyBoneKeys(float time, Dictionary<string, GameObject> rig)
        {
            ResolvePendingOriginals();
            var samples = ResolveBoneKeys(time);
            var touched = new HashSet<SpriteRenderer>();
            foreach (var (bone, sample) in samples)
            {
                if (!rig.TryGetValue(bone, out var obj))
                    obj = FindInRig(bone);
                var part = obj != null ? PartOf(obj) : null;
                if (part == null || !touched.Add(part))
                    continue;
                ApplyPart(part, sample);
            }

            // Partes que alguma chave mexeu antes mas não estão mais sendo animadas: restaura.
            foreach (var part in _overrides.Keys.ToList())
                if (!touched.Contains(part))
                    ApplyPart(part, default);
        }

        private void ApplyPart(SpriteRenderer part, BoneKeySample s)
        {
            _overrides.TryGetValue(part, out var ov);
            PartOverride Ensure() => ov ??= _overrides[part] = new PartOverride();

            if (s.Sprite is { } sprite)
            {
                var o = Ensure();
                if (!o.Sprite) { o.Texture = part.TexturePath; o.Source = part.SourceRect; o.Sprite = true; }
                var mapped = CurrentSkin?.Remap(sprite) ?? sprite;
                part.TexturePath = string.IsNullOrEmpty(mapped.TexturePath) ? o.Texture : mapped.TexturePath;
                part.SourceRect = mapped.SourceRect;
            }
            else if (ov is { Sprite: true })
            {
                part.TexturePath = ov.Texture;
                part.SourceRect = ov.Source;
                ov.Sprite = false;
            }

            if (s.SortOrder is { } sort)
            {
                var o = Ensure();
                if (!o.Sort) { o.SortOrder = part.Owner.SortOrder; o.Sort = true; }
                part.Owner.SortOrder = sort;
            }
            else if (ov is { Sort: true })
            {
                part.Owner.SortOrder = ov.SortOrder;
                ov.Sort = false;
            }

            if (s.Tint is { } tint)
            {
                var o = Ensure();
                if (!o.Tint) { o.TintColor = part.Tint; o.Tint = true; }
                part.Tint = tint;
            }
            else if (ov is { Tint: true })
            {
                part.Tint = ov.TintColor;
                ov.Tint = false;
            }

            if (s.Visible is { } visible)
            {
                var o = Ensure();
                if (!o.Visible) { o.Enabled = part.Enabled; o.Visible = true; }
                part.Enabled = visible;
            }
            else if (ov is { Visible: true })
            {
                part.Enabled = ov.Enabled;
                ov.Visible = false;
            }

            if (ov != null && !ov.Any)
                _overrides.Remove(part);
        }

        /// <summary>Devolve todas as partes aos valores originais (desfaz sprite/ordem/cor/
        /// visibilidade aplicados pelas chaves).</summary>
        public void RestoreParts()
        {
            ResolvePendingOriginals();
            foreach (var part in _overrides.Keys.ToList())
                ApplyPart(part, default);
        }

        /// <summary>Reaplica só sprite/ordem/cor/visibilidade das chaves no tempo atual (sem mexer
        /// nos transforms dos ossos) — usado pelo editor ao editar as chaves.</summary>
        public void RefreshBoneKeys()
        {
            if (Owner == null)
                return;
            if (_current.Keyframes.Count == 0)
                RestoreParts();
            else
                ApplyBoneKeys(_time, RigBones());
        }

        /// <summary>Valor original de uma parte enquanto uma chave a sobrepõe (nome do objeto da
        /// parte + valores; nulo = não sobreposto). Salvo na cena para o original não se perder
        /// ao salvar com o playhead parado numa chave.</summary>
        public readonly record struct PartOriginal(string Part, SpriteRef? Sprite, int? SortOrder, Color? Tint, bool? Enabled);

        private List<PartOriginal>? _pendingOriginals;

        /// <summary>Originais das partes sobrepostas agora (e os ainda não resolvidos).</summary>
        public IReadOnlyList<PartOriginal> GetPartOriginals()
        {
            var list = new List<PartOriginal>();
            foreach (var (part, ov) in _overrides)
            {
                if (part.Owner == null)
                    continue;
                list.Add(new PartOriginal(part.Owner.Name,
                    ov.Sprite ? new SpriteRef(ov.Texture, ov.Source) : null,
                    ov.Sort ? ov.SortOrder : null,
                    ov.Tint ? ov.TintColor : null,
                    ov.Visible ? ov.Enabled : null));
            }
            if (_pendingOriginals != null)
                foreach (var p in _pendingOriginals)
                    if (list.All(x => x.Part != p.Part))
                        list.Add(p);
            return list;
        }

        /// <summary>Registra originais lidos da cena; são ligados às partes (por nome) no primeiro uso.</summary>
        public void SetPartOriginals(IEnumerable<PartOriginal> originals)
        {
            _pendingOriginals = originals.ToList();
            if (_pendingOriginals.Count == 0)
                _pendingOriginals = null;
        }

        private void ResolvePendingOriginals()
        {
            if (_pendingOriginals == null || Owner == null)
                return;
            var unresolved = new List<PartOriginal>();
            foreach (var p in _pendingOriginals)
            {
                var part = FindInRig(p.Part)?.Components.OfType<SpriteRenderer>().FirstOrDefault();
                if (part == null)
                {
                    unresolved.Add(p);
                    continue;
                }
                if (_overrides.ContainsKey(part))
                    continue;
                var ov = new PartOverride();
                if (p.Sprite is { } s) { ov.Sprite = true; ov.Texture = s.TexturePath; ov.Source = s.SourceRect; }
                if (p.SortOrder is { } o) { ov.Sort = true; ov.SortOrder = o; }
                if (p.Tint is { } c) { ov.Tint = true; ov.TintColor = c; }
                if (p.Enabled is { } e) { ov.Visible = true; ov.Enabled = e; }
                if (ov.Any)
                    _overrides[part] = ov;
            }
            _pendingOriginals = unresolved.Count > 0 ? unresolved : null;
        }

        /// <summary>
        /// Matrizes de mundo de todos os objetos do rig na pose do tempo informado, sem mexer nos
        /// transforms (usado pelo onion skin do editor). Não inclui IK.
        /// </summary>
        public Dictionary<GameObject, Matrix> ComputeWorldMatrices(float time)
        {
            var result = new Dictionary<GameObject, Matrix>();
            if (Owner == null)
                return result;

            var pose = ResolvePose(time);
            var parentWorld = Owner.Parent?.Transform.WorldMatrix ?? Matrix.Identity;
            Walk(Owner, parentWorld);
            return result;

            void Walk(GameObject obj, Matrix parent)
            {
                var local = pose.TryGetValue(obj.Name, out var p) && obj.Components.OfType<Bone>().Any()
                    ? p.ToMatrix()
                    : obj.Transform.LocalMatrix;
                var world = local * parent;
                result[obj] = world;
                foreach (var child in obj.Children)
                    Walk(child, world);
            }
        }

        // ---- IK e pontos de fixação ----

        /// <summary>Aplica as restrições <see cref="IkConstraint"/> do rig (depois da pose).</summary>
        public void ApplyConstraints()
        {
            if (Owner == null)
                return;
            foreach (var ik in Descendants(Owner).SelectMany(o => o.Components.OfType<IkConstraint>()).ToList())
                if (ik.Enabled)
                    ik.Solve();
        }

        /// <summary>
        /// Posição/rotação de mundo de um ponto de fixação nomeado ("mao_d", "ponta_espada"):
        /// um <see cref="AttachPoint"/> do rig com esse nome ou, se não houver, um objeto do rig
        /// com esse nome. A rotação (radianos) considera espelhamento (escala negativa).
        /// </summary>
        public bool TryGetAttachPoint(string name, out Vector2 position, out float rotation)
        {
            var point = FindAttachPoint(name);
            if (point != null)
            {
                position = point.WorldPosition;
                rotation = point.WorldRotation;
                return true;
            }

            var obj = FindInRig(name);
            if (obj != null)
            {
                position = obj.Transform.WorldPosition;
                rotation = AttachPoint.WorldAngle(obj.Transform.WorldMatrix, 0f);
                return true;
            }

            position = Vector2.Zero;
            rotation = 0f;
            return false;
        }

        /// <summary>Componente <see cref="AttachPoint"/> do rig com o nome informado, ou nulo.</summary>
        public AttachPoint? FindAttachPoint(string name)
        {
            if (Owner == null)
                return null;
            foreach (var obj in Descendants(Owner))
                foreach (var c in obj.Components)
                    if (c is AttachPoint ap && ap.ResolvedName == name)
                        return ap;
            return null;
        }

        /// <summary>Todos os pontos de fixação do rig.</summary>
        public IEnumerable<AttachPoint> AttachPoints =>
            Owner == null ? Enumerable.Empty<AttachPoint>() : Descendants(Owner).SelectMany(o => o.Components.OfType<AttachPoint>());

        private static IEnumerable<GameObject> Descendants(GameObject root)
        {
            yield return root;
            foreach (var child in root.Children)
                foreach (var d in Descendants(child))
                    yield return d;
        }

        // ---- skins ----

        public IReadOnlyList<Skin> Skins => _skins;
        public IEnumerable<string> SkinNames => _skins.Select(s => s.Name);

        /// <summary>Skin ativa (nulo se nenhuma).</summary>
        public Skin? CurrentSkin => string.IsNullOrEmpty(_skin) ? null : GetSkin(_skin);

        public Skin? GetSkin(string name) => _skins.FirstOrDefault(s => s.Name == name);

        /// <summary>Nome da skin ativa (vazio = nenhuma). Trocar aplica todas as partes de uma vez.</summary>
        public string Skin
        {
            get => _skin;
            set
            {
                value ??= string.Empty;
                if (_skin == value)
                    return;
                _skin = value;
                OnPropertyChanged();
                ApplySkin();
            }
        }

        /// <summary>Adiciona (ou retorna, se já existe) uma skin vazia.</summary>
        public Skin AddSkin(string name)
        {
            name = string.IsNullOrWhiteSpace(name) ? "skin" : name.Trim();
            var skin = GetSkin(name);
            if (skin == null)
            {
                skin = new Skin(name);
                _skins.Add(skin);
                OnPropertyChanged(nameof(Skins));
                OnPropertyChanged(nameof(SkinNames));
            }
            return skin;
        }

        public bool RemoveSkin(string name)
        {
            var skin = GetSkin(name);
            if (skin == null)
                return false;
            _skins.Remove(skin);
            if (_skin == name)
            {
                _skin = string.Empty;
                OnPropertyChanged(nameof(Skin));
            }
            OnPropertyChanged(nameof(Skins));
            OnPropertyChanged(nameof(SkinNames));
            return true;
        }

        /// <summary>
        /// Grava na skin (cria se preciso) o sprite atual de cada parte do rig: dos ossos e dos
        /// objetos com SpriteRenderer. Fluxo do editor: pinte/troque as partes e capture a skin.
        /// </summary>
        public Skin CaptureSkin(string name)
        {
            var skin = AddSkin(name);
            skin.Parts.Clear();
            if (Owner == null)
                return skin;
            ResolvePendingOriginals();

            var seen = new HashSet<SpriteRenderer>();
            foreach (var obj in Descendants(Owner))
            {
                var part = PartOf(obj);
                if (part == null || part.Owner != obj && !obj.Components.OfType<Bone>().Any())
                    continue;
                if (!seen.Add(part))
                    continue;
                // Grava o original (não o sprite de uma chave de animação ativa).
                var (texture, source) = _overrides.TryGetValue(part, out var ov) && ov.Sprite
                    ? (ov.Texture, ov.Source)
                    : (part.TexturePath, part.SourceRect);
                skin.Parts[obj.Name] = new SpriteRef(texture, source);
            }
            return skin;
        }

        /// <summary>Aplica a skin ativa às partes do rig (e reaplica as trocas de sprite das chaves,
        /// para valerem os remapeamentos da skin).</summary>
        public void ApplySkin()
        {
            var skin = CurrentSkin;
            if (skin == null || Owner == null)
                return;
            ResolvePendingOriginals();

            foreach (var (slot, sprite) in skin.Parts)
            {
                var part = FindPart(slot);
                if (part == null)
                    continue;
                if (_overrides.TryGetValue(part, out var ov) && ov.Sprite)
                {
                    // Uma chave está sobrepondo: a skin vira o "original" a restaurar.
                    ov.Texture = sprite.TexturePath;
                    ov.Source = sprite.SourceRect;
                }
                else
                {
                    part.TexturePath = sprite.TexturePath;
                    part.SourceRect = sprite.SourceRect;
                }
            }

            if (_current.Keyframes.Count > 0)
                ApplyBoneKeys(_time, RigBones());
        }

        /// <summary>Reescreve os caminhos de textura das trocas de sprite (chaves) e das skins —
        /// usado ao exportar o jogo (copia os assets e troca para caminhos relativos).</summary>
        public void RemapSpritePaths(Func<string?, string?> map)
        {
            SpriteRef Map(SpriteRef s) => s with { TexturePath = string.IsNullOrEmpty(s.TexturePath) ? s.TexturePath : map(s.TexturePath) };

            foreach (var clip in _clips)
                foreach (var key in clip.Keyframes)
                    foreach (var bk in key.BoneKeys.Values)
                        if (bk.Sprite is { } sprite)
                            bk.Sprite = Map(sprite);

            foreach (var skin in _skins)
            {
                foreach (var slot in skin.Parts.Keys.ToList())
                    skin.Parts[slot] = Map(skin.Parts[slot]);
                foreach (var k in skin.Remaps.Keys.ToList())
                    skin.Remaps[k] = Map(skin.Remaps[k]);
            }

            foreach (var ov in _overrides.Values)
                if (ov.Sprite && !string.IsNullOrEmpty(ov.Texture))
                    ov.Texture = map(ov.Texture);
            if (_pendingOriginals != null)
                _pendingOriginals = _pendingOriginals
                    .Select(p => p.Sprite is { } s ? p with { Sprite = Map(s) } : p)
                    .ToList();
        }

        // ---- cópia e snapshot (duplicar objeto, undo do editor) ----

        /// <summary>Estado editável do animador (clipes, skins, ativo) para undo/redo.</summary>
        public sealed class Snapshot
        {
            internal Snapshot(List<PoseClip> clips, string current, List<Skin> skins, string skin)
            {
                Clips = clips; Current = current; Skins = skins; SkinName = skin;
            }
            internal List<PoseClip> Clips { get; }
            internal string Current { get; }
            internal List<Skin> Skins { get; }
            internal string SkinName { get; }
        }

        public Snapshot CreateSnapshot()
            => new(_clips.Select(c => c.Clone()).ToList(), _current.Name, _skins.Select(s => s.Clone()).ToList(), _skin);

        /// <summary>Restaura um snapshot (clipes, keyframes, extras e skins). Mantém o tempo atual.</summary>
        public void RestoreSnapshot(Snapshot snapshot)
        {
            _clips.Clear();
            _clips.AddRange(snapshot.Clips.Select(c => c.Clone()));
            if (_clips.Count == 0)
                _clips.Add(new PoseClip("default"));
            _current = _clips.FirstOrDefault(c => c.Name == snapshot.Current) ?? _clips[0];
            _time = Scrawlbit.Mathf.Clamp(_time, 0f, _current.Duration);

            _skins.Clear();
            _skins.AddRange(snapshot.Skins.Select(s => s.Clone()));
            _skin = snapshot.SkinName;

            OnPropertyChanged(nameof(Clips));
            OnPropertyChanged(nameof(ClipNames));
            OnPropertyChanged(nameof(CurrentClipName));
            OnPropertyChanged(nameof(Skins));
            OnPropertyChanged(nameof(SkinNames));
            OnPropertyChanged(nameof(Skin));
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(Loop));
            OnPropertyChanged(nameof(Easing));
            RaiseClipChanged();
        }

        /// <summary>Cópia independente do componente (clipes, extras, skins, velocidade).</summary>
        public SkeletonAnimator Clone()
        {
            var copy = new SkeletonAnimator { Enabled = Enabled, Speed = Speed };
            copy.RestoreSnapshot(CreateSnapshot());
            copy.SetPartOriginals(GetPartOriginals()); // as partes da cópia têm os mesmos nomes
            return copy;
        }

        // ---- ciclo de vida ----

        protected internal override void OnPlayStarted()
        {
            ResolvePendingOriginals(); // originais de partes que o editor deixou sobrepostas
            _time = 0f;
            _finished = false;
            _atClipStart = true;
            _queue.Clear();
            ApplySkin();
            _playing = _current.Keyframes.Count > 0;
            if (_playing)
                Sample(0f);
            else
                ApplyConstraints();
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (!_playing || _current.Keyframes.Count == 0)
            {
                ApplyConstraints(); // IK continua valendo com a animação parada
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds * _speed;
            if (_blendFrom != null)
            {
                _blendElapsed += dt;
                if (_blendElapsed >= _blendTime)
                    _blendFrom = null; // transição concluída
            }

            float prev = _atClipStart ? -1f : _time; // -1: inclui eventos em t = 0 no início do clipe
            _atClipStart = false;
            _time += dt;
            float duration = _current.Duration;

            if (_time >= duration && (!_current.Loop || _time > duration || _queue.Count > 0))
            {
                FireEventsInRange(prev, duration);
                var completed = _current.Name;

                if (_queue.Count > 0)
                {
                    var (next, blend) = _queue.Dequeue();
                    var clip = GetClip(next);
                    ClipCompleted?.Invoke(completed);
                    if (clip != null)
                    {
                        if (blend > 0f) _time = duration; // crossfade parte da pose final
                        StartClip(clip, blend);
                        Sample(0f);
                        return;
                    }
                }
                else if (_current.Loop)
                {
                    _time %= duration;
                    ClipCompleted?.Invoke(completed);
                    FireEventsInRange(-1f, _time);
                }
                else
                {
                    _time = duration;
                    _playing = false;
                    _finished = true;
                    ClipCompleted?.Invoke(completed);
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
            foreach (var (time, name) in _current.Events.ToList())
            {
                if (time > fromExclusive && time <= toInclusive)
                {
                    AnimationEvent?.Invoke(name);
                    if (_eventHandlers.TryGetValue(name, out var handlers))
                        foreach (var h in handlers.ToList())
                            h();
                    Owner?.Scene?.Send(name, Owner);
                }
            }
        }
    }
}
