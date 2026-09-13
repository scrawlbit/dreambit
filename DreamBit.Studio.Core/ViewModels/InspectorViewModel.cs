using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Notification;
using Microsoft.Xna.Framework;

namespace DreamBit.Studio.ViewModels
{
    /// <summary>
    /// Adaptador editável do objeto selecionado para o painel Inspetor: expõe X/Y,
    /// rotação (em graus) e escala como propriedades bindáveis, reconstruindo os
    /// Vector2 do Transform a cada alteração (structs não são bindáveis diretamente).
    /// </summary>
    public sealed class InspectorViewModel : NotificationObject
    {
        private GameObject? _target;

        public GameObject? Target
        {
            get => _target;
            set
            {
                if (_target == value)
                    return;

                if (_target != null)
                    _target.Transform.Changed -= RaiseAll;

                _target = value;

                if (_target != null)
                    _target.Transform.Changed += RaiseAll;

                RaiseAll();
            }
        }

        public bool HasTarget => _target != null;

        public string Name
        {
            get => _target?.Name ?? string.Empty;
            set { if (_target != null) { _target.Name = value; OnPropertyChanged(); } }
        }

        public string Tag
        {
            get => _target?.Tag ?? string.Empty;
            set { if (_target != null) { _target.Tag = value; OnPropertyChanged(); } }
        }

        public float PositionX
        {
            get => _target?.Transform.Position.X ?? 0f;
            set { if (_target != null) _target.Transform.Position = new Vector2(value, _target.Transform.Position.Y); }
        }

        public float PositionY
        {
            get => _target?.Transform.Position.Y ?? 0f;
            set { if (_target != null) _target.Transform.Position = new Vector2(_target.Transform.Position.X, value); }
        }

        public float RotationDegrees
        {
            get => _target != null ? MathHelper.ToDegrees(_target.Transform.Rotation) : 0f;
            set { if (_target != null) _target.Transform.Rotation = MathHelper.ToRadians(value); }
        }

        public float ScaleX
        {
            get => _target?.Transform.Scale.X ?? 1f;
            set { if (_target != null) _target.Transform.Scale = new Vector2(value, _target.Transform.Scale.Y); }
        }

        public float ScaleY
        {
            get => _target?.Transform.Scale.Y ?? 1f;
            set { if (_target != null) _target.Transform.Scale = new Vector2(_target.Transform.Scale.X, value); }
        }

        // ---- Componente SpriteRenderer ----

        private SpriteRenderer? Sprite => _target?.Components.OfType<SpriteRenderer>().FirstOrDefault();
        public bool HasSprite => Sprite != null;

        public float SpriteWidth
        {
            get => Sprite?.Size.X ?? 0f;
            set { var s = Sprite; if (s != null) s.Size = new Vector2(value, s.Size.Y); }
        }
        public float SpriteHeight
        {
            get => Sprite?.Size.Y ?? 0f;
            set { var s = Sprite; if (s != null) s.Size = new Vector2(s.Size.X, value); }
        }
        public int SpriteR
        {
            get => Sprite?.Color.R ?? 0;
            set { var s = Sprite; if (s != null) s.Color = new Color((byte)Clamp(value), s.Color.G, s.Color.B); }
        }
        public int SpriteG
        {
            get => Sprite?.Color.G ?? 0;
            set { var s = Sprite; if (s != null) s.Color = new Color(s.Color.R, (byte)Clamp(value), s.Color.B); }
        }
        public int SpriteB
        {
            get => Sprite?.Color.B ?? 0;
            set { var s = Sprite; if (s != null) s.Color = new Color(s.Color.R, s.Color.G, (byte)Clamp(value)); }
        }

        public string SpriteTexturePath
        {
            get => Sprite?.TexturePath ?? string.Empty;
            set
            {
                var s = Sprite;
                if (s != null)
                {
                    s.TexturePath = string.IsNullOrWhiteSpace(value) ? null : value;
                    Refresh();
                }
            }
        }
        public bool SpriteHasTexture => !string.IsNullOrEmpty(Sprite?.TexturePath);

        // Recorte no atlas (source rect). W/H = 0 => textura inteira.
        private Microsoft.Xna.Framework.Rectangle SpriteSource => Sprite?.SourceRect ?? Microsoft.Xna.Framework.Rectangle.Empty;
        private void SetSpriteSource(int x, int y, int w, int h)
        {
            var s = Sprite;
            if (s == null) return;
            s.SourceRect = w > 0 && h > 0 ? new Microsoft.Xna.Framework.Rectangle(x, y, w, h) : null;
            OnPropertyChanged(nameof(SpriteSrcX));
            OnPropertyChanged(nameof(SpriteSrcY));
            OnPropertyChanged(nameof(SpriteSrcW));
            OnPropertyChanged(nameof(SpriteSrcH));
        }
        public int SpriteSrcX { get => SpriteSource.X; set => SetSpriteSource(value, SpriteSource.Y, SpriteSource.Width, SpriteSource.Height); }
        public int SpriteSrcY { get => SpriteSource.Y; set => SetSpriteSource(SpriteSource.X, value, SpriteSource.Width, SpriteSource.Height); }
        public int SpriteSrcW { get => SpriteSource.Width; set => SetSpriteSource(SpriteSource.X, SpriteSource.Y, value, SpriteSource.Height); }
        public int SpriteSrcH { get => SpriteSource.Height; set => SetSpriteSource(SpriteSource.X, SpriteSource.Y, SpriteSource.Width, value); }

        // ---- Componente SpriteAnimator ----

        private SpriteAnimator? Animator => _target?.Components.OfType<SpriteAnimator>().FirstOrDefault();
        public bool HasAnimator => Animator != null;

        public string AnimTexturePath
        {
            get => Animator?.TexturePath ?? string.Empty;
            set { var a = Animator; if (a != null) { a.TexturePath = string.IsNullOrWhiteSpace(value) ? null : value; Refresh(); } }
        }
        public int AnimFrameWidth
        {
            get => Animator?.FrameWidth ?? 0;
            set { var a = Animator; if (a != null) a.FrameWidth = value; }
        }
        public int AnimFrameHeight
        {
            get => Animator?.FrameHeight ?? 0;
            set { var a = Animator; if (a != null) a.FrameHeight = value; }
        }
        public int AnimFrameCount
        {
            get => Animator?.FrameCount ?? 0;
            set { var a = Animator; if (a != null) a.FrameCount = value; }
        }
        public float AnimFps
        {
            get => Animator?.Fps ?? 0f;
            set { var a = Animator; if (a != null) a.Fps = value; }
        }
        public bool AnimLoop
        {
            get => Animator?.Loop ?? false;
            set { var a = Animator; if (a != null) a.Loop = value; }
        }

        /// <summary>
        /// Eventos de animação em texto: uma linha por evento no formato "frame: nome"
        /// (ex.: "0: passo"). Disparam ao a animação entrar naquele frame no play.
        /// </summary>
        public string AnimEventsText
        {
            get
            {
                var a = Animator;
                if (a == null) return string.Empty;
                return string.Join("\n", a.Events
                    .OrderBy(e => e.Frame)
                    .Select(e => $"{e.Frame}: {e.Name}"));
            }
            set
            {
                var a = Animator;
                if (a == null) return;

                var parsed = new List<AnimationFrameEvent>();
                foreach (var raw in (value ?? string.Empty).Split('\n'))
                {
                    var line = raw.Trim();
                    if (line.Length == 0) continue;

                    int sep = line.IndexOf(':');
                    if (sep <= 0) continue;

                    if (int.TryParse(line[..sep].Trim(), out int frame))
                    {
                        var name = line[(sep + 1)..].Trim();
                        if (name.Length > 0)
                            parsed.Add(new AnimationFrameEvent(frame, name));
                    }
                }
                a.SetEvents(parsed);
                OnPropertyChanged();
            }
        }

        // ---- Componente PlatformerController ----

        private PlatformerController? Platformer => _target?.Components.OfType<PlatformerController>().FirstOrDefault();
        public bool HasPlatformer => Platformer != null;

        public float PlatGravity
        {
            get => Platformer?.Gravity ?? 0f;
            set { var p = Platformer; if (p != null) p.Gravity = value; }
        }
        public float PlatHalfHeight
        {
            get => Platformer?.HalfHeight ?? 0f;
            set { var p = Platformer; if (p != null) p.HalfHeight = value; }
        }
        public float PlatHorizontalSpeed
        {
            get => Platformer?.HorizontalSpeed ?? 0f;
            set { var p = Platformer; if (p != null) p.HorizontalSpeed = value; }
        }
        public bool PlatUseKeyboard
        {
            get => Platformer?.UseKeyboard ?? false;
            set { var p = Platformer; if (p != null) { p.UseKeyboard = value; Refresh(); } }
        }
        public float PlatMoveSpeed
        {
            get => Platformer?.MoveSpeed ?? 0f;
            set { var p = Platformer; if (p != null) p.MoveSpeed = value; }
        }
        public float PlatJumpSpeed
        {
            get => Platformer?.JumpSpeed ?? 0f;
            set { var p = Platformer; if (p != null) p.JumpSpeed = value; }
        }

        // ---- Componente Script (C# em runtime) ----

        private ScriptComponent? Script => _target?.Components.OfType<ScriptComponent>().FirstOrDefault();
        public bool HasScript => Script != null;

        public string ScriptSource
        {
            get => Script?.Source ?? string.Empty;
            set { var s = Script; if (s != null) s.Source = value; }
        }
        public string ScriptError => Script?.Error ?? string.Empty;

        public void CompileScript()
        {
            Script?.Compile();
            Refresh();
        }

        // ---- Componente TriggerZone (coletável) ----

        private TriggerZone? Trigger => _target?.Components.OfType<TriggerZone>().FirstOrDefault();
        public bool HasTrigger => Trigger != null;

        public float TriggerWidth
        {
            get => Trigger?.Size.X ?? 0f;
            set { var t = Trigger; if (t != null) t.Size = new Vector2(value, t.Size.Y); }
        }
        public float TriggerHeight
        {
            get => Trigger?.Size.Y ?? 0f;
            set { var t = Trigger; if (t != null) t.Size = new Vector2(t.Size.X, value); }
        }
        public string TriggerTargetTag
        {
            get => Trigger?.TargetTag ?? string.Empty;
            set { var t = Trigger; if (t != null) t.TargetTag = value; }
        }
        public string TriggerSendOnEnter
        {
            get => Trigger?.SendOnEnter ?? string.Empty;
            set { var t = Trigger; if (t != null) t.SendOnEnter = value; }
        }

        // ---- Componente MessageListener (barramento de eventos) ----

        private MessageListener? Listener => _target?.Components.OfType<MessageListener>().FirstOrDefault();
        public bool HasListener => Listener != null;

        public string ListenerMessage
        {
            get => Listener?.Message ?? string.Empty;
            set { var l = Listener; if (l != null) l.Message = value; }
        }
        public MessageReaction ListenerReaction
        {
            get => Listener?.Reaction ?? MessageReaction.None;
            set { var l = Listener; if (l != null) l.Reaction = value; }
        }
        public System.Collections.Generic.IReadOnlyList<MessageReaction> ListenerReactions { get; }
            = Scrawlbit.EnumHelper.Values<MessageReaction>();
        public bool TriggerDestroyOnEnter
        {
            get => Trigger?.DestroyOnEnter ?? false;
            set { var t = Trigger; if (t != null) t.DestroyOnEnter = value; }
        }

        // ---- Componente FollowTarget (referência a outro objeto) ----

        private FollowTarget? Follow => _target?.Components.OfType<FollowTarget>().FirstOrDefault();
        public bool HasFollow => Follow != null;

        public GameObject? FollowTargetObject
        {
            get
            {
                var follow = Follow;
                if (follow == null || _target?.Scene == null || follow.TargetId == Guid.Empty)
                    return null;
                return FindById(_target.Scene.Objects, follow.TargetId);
            }
            set { var f = Follow; if (f != null) { f.TargetId = value?.Id ?? Guid.Empty; Refresh(); } }
        }

        public float FollowSpeed
        {
            get => Follow?.Speed ?? 0f;
            set { var f = Follow; if (f != null) f.Speed = value; }
        }

        private static GameObject? FindById(IEnumerable<GameObject> objects, Guid id)
        {
            foreach (var obj in objects)
            {
                if (obj.Id == id)
                    return obj;
                var nested = FindById(obj.Children, id);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        // ---- Componente ParticleEmitter ----

        private ParticleEmitter? Particles => _target?.Components.OfType<ParticleEmitter>().FirstOrDefault();
        public bool HasParticles => Particles != null;

        public float ParticleRate { get => Particles?.EmitRate ?? 0f; set { var p = Particles; if (p != null) p.EmitRate = value; } }
        public float ParticleLifetime { get => Particles?.Lifetime ?? 0f; set { var p = Particles; if (p != null) p.Lifetime = value; } }
        public float ParticleSpeed { get => Particles?.Speed ?? 0f; set { var p = Particles; if (p != null) p.Speed = value; } }
        public float ParticleSize { get => Particles?.Size ?? 0f; set { var p = Particles; if (p != null) p.Size = value; } }
        public float ParticleGravity { get => Particles?.GravityY ?? 0f; set { var p = Particles; if (p != null) p.GravityY = value; } }
        public int ParticleR { get => Particles?.Color.R ?? 0; set { var p = Particles; if (p != null) p.Color = new Color((byte)Clamp(value), p.Color.G, p.Color.B); } }
        public int ParticleG { get => Particles?.Color.G ?? 0; set { var p = Particles; if (p != null) p.Color = new Color(p.Color.R, (byte)Clamp(value), p.Color.B); } }
        public int ParticleB { get => Particles?.Color.B ?? 0; set { var p = Particles; if (p != null) p.Color = new Color(p.Color.R, p.Color.G, (byte)Clamp(value)); } }

        // ---- Componente AudioSource ----

        private AudioSource? Audio => _target?.Components.OfType<AudioSource>().FirstOrDefault();
        public bool HasAudio => Audio != null;

        public string AudioPath
        {
            get => Audio?.SoundPath ?? string.Empty;
            set { var a = Audio; if (a != null) { a.SoundPath = string.IsNullOrWhiteSpace(value) ? null : value; Refresh(); } }
        }
        public float AudioVolume
        {
            get => Audio?.Volume ?? 1f;
            set { var a = Audio; if (a != null) a.Volume = value; }
        }
        public bool AudioPlayOnStart
        {
            get => Audio?.PlayOnStart ?? false;
            set { var a = Audio; if (a != null) a.PlayOnStart = value; }
        }
        public bool AudioLoop
        {
            get => Audio?.Loop ?? false;
            set { var a = Audio; if (a != null) a.Loop = value; }
        }

        // ---- Componente TilemapRenderer ----

        private TilemapRenderer? Tilemap => _target?.Components.OfType<TilemapRenderer>().FirstOrDefault();
        public bool HasTilemap => Tilemap != null;

        public string TilemapPath
        {
            get => Tilemap?.TmxPath ?? string.Empty;
            set { var t = Tilemap; if (t != null) { t.TmxPath = string.IsNullOrWhiteSpace(value) ? null : value; Refresh(); } }
        }

        // ---- Componente Bone (rig cutout) ----

        private Bone? Bone => _target?.Components.OfType<Bone>().FirstOrDefault();
        public bool HasBone => Bone != null;

        public float BoneLength
        {
            get => Bone?.Length ?? 0f;
            set { var b = Bone; if (b != null) b.Length = value; }
        }
        public bool BoneHasRestPose => Bone?.HasRestPose ?? false;

        public void CaptureBonePose()
        {
            Bone?.CaptureRestPose();
            OnPropertyChanged(nameof(BoneHasRestPose));
        }

        public void ResetBonePose()
        {
            Bone?.ResetToRestPose();
            RaiseAll(); // posição/rotação/escala mudaram
        }

        // ---- Componente SkeletonAnimator (keyframes de pose) ----

        private SkeletonAnimator? Skeleton => _target?.Components.OfType<SkeletonAnimator>().FirstOrDefault();
        public bool HasSkeleton => Skeleton != null;

        public float SkeletonDuration
        {
            get => Skeleton?.Duration ?? 1f;
            set { var s = Skeleton; if (s != null) { s.Duration = value; OnPropertyChanged(); OnPropertyChanged(nameof(SkeletonTime)); } }
        }
        public bool SkeletonLoop
        {
            get => Skeleton?.Loop ?? true;
            set { var s = Skeleton; if (s != null) s.Loop = value; }
        }
        public int SkeletonKeyframeCount => Skeleton?.KeyframeCount ?? 0;

        /// <summary>Tempo atual do clipe (scrub). Aplica a pose interpolada aos ossos.</summary>
        public float SkeletonTime
        {
            get => Skeleton?.Time ?? 0f;
            set { var s = Skeleton; if (s != null) { s.SetTime(value); OnPropertyChanged(); } }
        }

        /// <summary>Tempos (segundos) dos keyframes, para exibir/pular na timeline.</summary>
        public IEnumerable<float> SkeletonKeyframeTimes =>
            Skeleton?.Keyframes.Select(k => k.Time).ToList() ?? Enumerable.Empty<float>();

        /// <summary>Captura a pose atual dos ossos como keyframe no tempo atual.</summary>
        public void AddSkeletonKeyframe()
        {
            var s = Skeleton;
            if (s == null) return;
            s.CaptureKeyframe(s.Time);
            OnPropertyChanged(nameof(SkeletonKeyframeCount));
            OnPropertyChanged(nameof(SkeletonKeyframeTimes));
        }

        /// <summary>Remove o keyframe mais próximo do tempo atual.</summary>
        public void RemoveSkeletonKeyframe()
        {
            var s = Skeleton;
            if (s == null) return;
            s.RemoveKeyframeNear(s.Time);
            OnPropertyChanged(nameof(SkeletonKeyframeCount));
            OnPropertyChanged(nameof(SkeletonKeyframeTimes));
        }

        /// <summary>Move o playhead para um tempo específico (pular para um keyframe).</summary>
        public void GoToSkeletonTime(float time)
        {
            SkeletonTime = time;
        }

        // ---- Componente RotatorBehavior ----

        private RotatorBehavior? Rotator => _target?.Components.OfType<RotatorBehavior>().FirstOrDefault();
        public bool HasRotator => Rotator != null;

        public float RotatorSpeed
        {
            get => Rotator?.Speed ?? 0f;
            set { var r = Rotator; if (r != null) r.Speed = value; }
        }

        /// <summary>Reavalia todos os campos (ex.: após adicionar/remover componente).</summary>
        public void Refresh() => RaiseAll();

        private static int Clamp(int value) => value < 0 ? 0 : value > 255 ? 255 : value;

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(HasTarget));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Tag));
            OnPropertyChanged(nameof(PositionX));
            OnPropertyChanged(nameof(PositionY));
            OnPropertyChanged(nameof(RotationDegrees));
            OnPropertyChanged(nameof(ScaleX));
            OnPropertyChanged(nameof(ScaleY));
            OnPropertyChanged(nameof(HasSprite));
            OnPropertyChanged(nameof(SpriteWidth));
            OnPropertyChanged(nameof(SpriteHeight));
            OnPropertyChanged(nameof(SpriteR));
            OnPropertyChanged(nameof(SpriteG));
            OnPropertyChanged(nameof(SpriteB));
            OnPropertyChanged(nameof(SpriteTexturePath));
            OnPropertyChanged(nameof(SpriteHasTexture));
            OnPropertyChanged(nameof(SpriteSrcX));
            OnPropertyChanged(nameof(SpriteSrcY));
            OnPropertyChanged(nameof(SpriteSrcW));
            OnPropertyChanged(nameof(SpriteSrcH));
            OnPropertyChanged(nameof(HasScript));
            OnPropertyChanged(nameof(ScriptSource));
            OnPropertyChanged(nameof(ScriptError));
            OnPropertyChanged(nameof(HasTrigger));
            OnPropertyChanged(nameof(TriggerWidth));
            OnPropertyChanged(nameof(TriggerHeight));
            OnPropertyChanged(nameof(TriggerTargetTag));
            OnPropertyChanged(nameof(TriggerDestroyOnEnter));
            OnPropertyChanged(nameof(TriggerSendOnEnter));
            OnPropertyChanged(nameof(HasListener));
            OnPropertyChanged(nameof(ListenerMessage));
            OnPropertyChanged(nameof(ListenerReaction));
            OnPropertyChanged(nameof(HasFollow));
            OnPropertyChanged(nameof(FollowTargetObject));
            OnPropertyChanged(nameof(FollowSpeed));
            OnPropertyChanged(nameof(HasParticles));
            OnPropertyChanged(nameof(ParticleRate));
            OnPropertyChanged(nameof(ParticleLifetime));
            OnPropertyChanged(nameof(ParticleSpeed));
            OnPropertyChanged(nameof(ParticleSize));
            OnPropertyChanged(nameof(ParticleGravity));
            OnPropertyChanged(nameof(ParticleR));
            OnPropertyChanged(nameof(ParticleG));
            OnPropertyChanged(nameof(ParticleB));
            OnPropertyChanged(nameof(HasAudio));
            OnPropertyChanged(nameof(AudioPath));
            OnPropertyChanged(nameof(AudioVolume));
            OnPropertyChanged(nameof(AudioPlayOnStart));
            OnPropertyChanged(nameof(AudioLoop));
            OnPropertyChanged(nameof(HasTilemap));
            OnPropertyChanged(nameof(TilemapPath));
            OnPropertyChanged(nameof(HasPlatformer));
            OnPropertyChanged(nameof(PlatGravity));
            OnPropertyChanged(nameof(PlatHalfHeight));
            OnPropertyChanged(nameof(PlatHorizontalSpeed));
            OnPropertyChanged(nameof(PlatUseKeyboard));
            OnPropertyChanged(nameof(PlatMoveSpeed));
            OnPropertyChanged(nameof(PlatJumpSpeed));
            OnPropertyChanged(nameof(HasAnimator));
            OnPropertyChanged(nameof(AnimTexturePath));
            OnPropertyChanged(nameof(AnimFrameWidth));
            OnPropertyChanged(nameof(AnimFrameHeight));
            OnPropertyChanged(nameof(AnimFrameCount));
            OnPropertyChanged(nameof(AnimFps));
            OnPropertyChanged(nameof(AnimLoop));
            OnPropertyChanged(nameof(AnimEventsText));
            OnPropertyChanged(nameof(HasRotator));
            OnPropertyChanged(nameof(RotatorSpeed));
            OnPropertyChanged(nameof(HasBone));
            OnPropertyChanged(nameof(BoneLength));
            OnPropertyChanged(nameof(BoneHasRestPose));
            OnPropertyChanged(nameof(HasSkeleton));
            OnPropertyChanged(nameof(SkeletonDuration));
            OnPropertyChanged(nameof(SkeletonLoop));
            OnPropertyChanged(nameof(SkeletonTime));
            OnPropertyChanged(nameof(SkeletonKeyframeCount));
            OnPropertyChanged(nameof(SkeletonKeyframeTimes));
        }
    }
}
