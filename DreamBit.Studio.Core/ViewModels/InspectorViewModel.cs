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

        /// <summary>Ordem de desenho (z-order): menor atrás, maior na frente.</summary>
        public int SortOrder
        {
            get => _target?.SortOrder ?? 0;
            set { if (_target != null) { _target.SortOrder = value; OnPropertyChanged(); } }
        }

        /// <summary>Camada de render (grossa): desenhada antes do z-order.</summary>
        public int RenderLayer
        {
            get => _target?.RenderLayer ?? 0;
            set { if (_target != null) { _target.RenderLayer = value; OnPropertyChanged(); } }
        }

        /// <summary>Fixo na tela (HUD): não anda com a câmera; a posição vira coordenada de tela.</summary>
        public bool ScreenSpace
        {
            get => _target?.ScreenSpace ?? false;
            set { if (_target != null) { _target.ScreenSpace = value; OnPropertyChanged(); } }
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

        // Chroma key do sprite (remover cor de fundo).
        public bool SpriteChroma { get => Sprite?.ChromaKeyEnabled ?? false; set { var s = Sprite; if (s != null) { s.ChromaKeyEnabled = value; OnPropertyChanged(); } } }
        public bool SpriteChromaAuto { get => Sprite?.ChromaAuto ?? true; set { var s = Sprite; if (s != null) { s.ChromaAuto = value; OnPropertyChanged(); } } }
        public int SpriteChromaR { get => Sprite?.ChromaColor.R ?? 255; set { var s = Sprite; if (s != null) s.ChromaColor = new Color((byte)Clamp(value), s.ChromaColor.G, s.ChromaColor.B); } }
        public int SpriteChromaG { get => Sprite?.ChromaColor.G ?? 0; set { var s = Sprite; if (s != null) s.ChromaColor = new Color(s.ChromaColor.R, (byte)Clamp(value), s.ChromaColor.B); } }
        public int SpriteChromaB { get => Sprite?.ChromaColor.B ?? 255; set { var s = Sprite; if (s != null) s.ChromaColor = new Color(s.ChromaColor.R, s.ChromaColor.G, (byte)Clamp(value)); } }
        public int SpriteChromaTolerance { get => Sprite?.ChromaTolerance ?? 30; set { var s = Sprite; if (s != null) s.ChromaTolerance = value; } }

        /// <summary>Define a cor de fundo do chroma do sprite (usado pelo botão "detectar cor").</summary>
        public void SetSpriteChroma(Color color)
        {
            var s = Sprite;
            if (s == null) return;
            s.ChromaColor = color;
            s.ChromaAuto = false;
            RaiseAll();
        }

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

        // Chroma key do animator.
        public bool AnimChroma { get => Animator?.ChromaKeyEnabled ?? false; set { var a = Animator; if (a != null) { a.ChromaKeyEnabled = value; OnPropertyChanged(); } } }
        public bool AnimChromaAuto { get => Animator?.ChromaAuto ?? true; set { var a = Animator; if (a != null) { a.ChromaAuto = value; OnPropertyChanged(); } } }
        public int AnimChromaR { get => Animator?.ChromaColor.R ?? 255; set { var a = Animator; if (a != null) a.ChromaColor = new Color((byte)Clamp(value), a.ChromaColor.G, a.ChromaColor.B); } }
        public int AnimChromaG { get => Animator?.ChromaColor.G ?? 0; set { var a = Animator; if (a != null) a.ChromaColor = new Color(a.ChromaColor.R, (byte)Clamp(value), a.ChromaColor.B); } }
        public int AnimChromaB { get => Animator?.ChromaColor.B ?? 255; set { var a = Animator; if (a != null) a.ChromaColor = new Color(a.ChromaColor.R, a.ChromaColor.G, (byte)Clamp(value)); } }
        public int AnimChromaTolerance { get => Animator?.ChromaTolerance ?? 30; set { var a = Animator; if (a != null) a.ChromaTolerance = value; } }

        public void SetAnimChroma(Color color)
        {
            var a = Animator;
            if (a == null) return;
            a.ChromaColor = color;
            a.ChromaAuto = false;
            RaiseAll();
        }

        /// <summary>Quantos frames explícitos (detectados) o animator usa; 0 = grade uniforme.</summary>
        public int AnimDetectedFrames => Animator?.Frames.Count ?? 0;
        public bool AnimUsesDetectedFrames => AnimDetectedFrames > 0;

        /// <summary>Aplica frames explícitos (autodetecção por transparência). Vazio volta à grade.</summary>
        public void ApplyDetectedFrames(System.Collections.Generic.IReadOnlyList<Rectangle> rects)
        {
            var a = Animator;
            if (a == null)
                return;
            a.SetFrames(rects);
            if (rects.Count > 0)
                a.FrameCount = rects.Count;
            Refresh();
        }

        /// <summary>Descarta os frames detectados e volta à grade uniforme.</summary>
        public void ClearDetectedFrames()
        {
            Animator?.SetFrames(System.Array.Empty<Rectangle>());
            Refresh();
        }

        // Fatiador por grade: informa colunas×linhas e deduz frame width/height/count da imagem.
        private int _animCols = 1;
        private int _animRows = 1;
        public int AnimCols { get => _animCols; set { _animCols = value < 1 ? 1 : value; OnPropertyChanged(); } }
        public int AnimRows { get => _animRows; set { _animRows = value < 1 ? 1 : value; OnPropertyChanged(); } }

        /// <summary>Fatia a sprite sheet do animator em <see cref="AnimCols"/>×<see cref="AnimRows"/>
        /// células, deduzindo largura/altura/quantidade de frame do tamanho do PNG.</summary>
        public void SliceAnimatorGrid()
        {
            var a = Animator;
            if (a == null || string.IsNullOrEmpty(a.TexturePath))
                return;

            var (w, h) = DreamBit.Studio.ImageInfo.GetPngSize(a.TexturePath!);
            if (w <= 0 || h <= 0)
                return;

            a.FrameWidth = w / _animCols;
            a.FrameHeight = h / _animRows;
            a.FrameCount = _animCols * _animRows;
            Refresh();
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
        public float PlatHalfWidth
        {
            get => Platformer?.HalfWidth ?? 0f;
            set { var p = Platformer; if (p != null) p.HalfWidth = value; }
        }

        // ---- Componente BoxCollider (sólido) ----

        private BoxCollider? Collider => _target?.Components.OfType<BoxCollider>().FirstOrDefault();
        public bool HasCollider => Collider != null;

        public float ColliderWidth
        {
            get => Collider?.Size.X ?? 0f;
            set { var c = Collider; if (c != null) c.Size = new Vector2(value, c.Size.Y); }
        }
        public float ColliderHeight
        {
            get => Collider?.Size.Y ?? 0f;
            set { var c = Collider; if (c != null) c.Size = new Vector2(c.Size.X, value); }
        }
        public float ColliderOffsetX
        {
            get => Collider?.Offset.X ?? 0f;
            set { var c = Collider; if (c != null) c.Offset = new Vector2(value, c.Offset.Y); }
        }
        public float ColliderOffsetY
        {
            get => Collider?.Offset.Y ?? 0f;
            set { var c = Collider; if (c != null) c.Offset = new Vector2(c.Offset.X, value); }
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
        public string ScriptSourcePath
        {
            get => Script?.SourcePath ?? string.Empty;
            set { var s = Script; if (s != null) { s.SourcePath = value; Refresh(); } }
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

        // ---- Componente AnimatorController ----

        private AnimatorController? AnimController => _target?.Components.OfType<AnimatorController>().FirstOrDefault();
        public bool HasAnimController => AnimController != null;
        public string CtrlIdleClip { get => AnimController?.IdleClip ?? string.Empty; set { var c = AnimController; if (c != null) c.IdleClip = value; } }
        public string CtrlWalkClip { get => AnimController?.WalkClip ?? string.Empty; set { var c = AnimController; if (c != null) c.WalkClip = value; } }
        public string CtrlJumpClip { get => AnimController?.JumpClip ?? string.Empty; set { var c = AnimController; if (c != null) c.JumpClip = value; } }
        public float CtrlBlendTime { get => AnimController?.BlendTime ?? 0.15f; set { var c = AnimController; if (c != null) c.BlendTime = value; } }

        // ---- Componente TweenComponent ----

        private TweenComponent? Tween => _target?.Components.OfType<TweenComponent>().FirstOrDefault();
        public bool HasTween => Tween != null;

        public TweenChannel TweenChannel { get => Tween?.Channel ?? DreamBit.Engine.Components.TweenChannel.PositionY; set { var t = Tween; if (t != null) t.Channel = value; } }
        public System.Collections.Generic.IReadOnlyList<TweenChannel> TweenChannels { get; } = Scrawlbit.EnumHelper.Values<TweenChannel>();
        public TweenLoop TweenLoop { get => Tween?.Loop ?? DreamBit.Engine.Components.TweenLoop.PingPong; set { var t = Tween; if (t != null) t.Loop = value; } }
        public System.Collections.Generic.IReadOnlyList<TweenLoop> TweenLoops { get; } = Scrawlbit.EnumHelper.Values<TweenLoop>();
        public Scrawlbit.EasingMode TweenEasing { get => Tween?.Easing ?? Scrawlbit.EasingMode.InOut; set { var t = Tween; if (t != null) t.Easing = value; } }
        public float TweenFrom { get => Tween?.From ?? 0f; set { var t = Tween; if (t != null) t.From = value; } }
        public float TweenTo { get => Tween?.To ?? 0f; set { var t = Tween; if (t != null) t.To = value; } }
        public float TweenDuration { get => Tween?.Duration ?? 1f; set { var t = Tween; if (t != null) t.Duration = value; } }
        public bool TweenPlayOnStart { get => Tween?.PlayOnStart ?? true; set { var t = Tween; if (t != null) t.PlayOnStart = value; } }

        // ---- Componente TextRenderer ----

        private TextRenderer? Text => _target?.Components.OfType<TextRenderer>().FirstOrDefault();
        public bool HasText => Text != null;

        public string TextValue
        {
            get => Text?.Text ?? string.Empty;
            set { var t = Text; if (t != null) t.Text = value; }
        }
        public string TextLocKey
        {
            get => Text?.LocKey ?? string.Empty;
            set { var t = Text; if (t != null) t.LocKey = value; }
        }
        public int TextPixelSize
        {
            get => Text?.PixelSize ?? 4;
            set { var t = Text; if (t != null) t.PixelSize = value; }
        }
        public bool TextScreenSpace
        {
            get => Text?.ScreenSpace ?? false;
            set { var t = Text; if (t != null) t.ScreenSpace = value; }
        }
        public int TextR { get => Text?.Color.R ?? 255; set { var t = Text; if (t != null) t.Color = new Color((byte)Clamp(value), t.Color.G, t.Color.B); } }
        public int TextG { get => Text?.Color.G ?? 255; set { var t = Text; if (t != null) t.Color = new Color(t.Color.R, (byte)Clamp(value), t.Color.B); } }
        public int TextB { get => Text?.Color.B ?? 255; set { var t = Text; if (t != null) t.Color = new Color(t.Color.R, t.Color.G, (byte)Clamp(value)); } }

        // ---- Componente CameraComponent ----

        private CameraComponent? Camera => _target?.Components.OfType<CameraComponent>().FirstOrDefault();
        public bool HasCamera => Camera != null;

        public string CameraTargetTag
        {
            get => Camera?.TargetTag ?? string.Empty;
            set { var c = Camera; if (c != null) c.TargetTag = value; }
        }
        public float CameraDeadzoneWidth
        {
            get => Camera?.DeadzoneWidth ?? 0f;
            set { var c = Camera; if (c != null) c.DeadzoneWidth = value; }
        }
        public float CameraDeadzoneHeight
        {
            get => Camera?.DeadzoneHeight ?? 0f;
            set { var c = Camera; if (c != null) c.DeadzoneHeight = value; }
        }
        public float CameraSmoothTime
        {
            get => Camera?.SmoothTime ?? 0f;
            set { var c = Camera; if (c != null) c.SmoothTime = value; }
        }
        public float CameraZoom
        {
            get => Camera?.Zoom ?? 1f;
            set { var c = Camera; if (c != null) c.Zoom = value; }
        }
        public bool CameraUseBounds
        {
            get => Camera?.UseBounds ?? false;
            set { var c = Camera; if (c != null) c.UseBounds = value; }
        }
        public int CameraPriority { get => Camera?.Priority ?? 0; set { var c = Camera; if (c != null) c.Priority = value; } }
        public string CameraShakeOn { get => Camera?.ShakeOnMessage ?? ""; set { var c = Camera; if (c != null) c.ShakeOnMessage = value; } }
        public float CameraShakeDuration { get => Camera?.ShakeMessageDuration ?? 0.3f; set { var c = Camera; if (c != null) c.ShakeMessageDuration = value; } }
        public float CameraShakeMagnitude { get => Camera?.ShakeMessageMagnitude ?? 12f; set { var c = Camera; if (c != null) c.ShakeMessageMagnitude = value; } }

        // ---- Componente PropertyAnimator (timeline de propriedades) ----

        private PropertyAnimator? PropAnim => _target?.Components.OfType<PropertyAnimator>().FirstOrDefault();
        public bool HasPropAnim => PropAnim != null;

        public float PropAnimDuration
        {
            get => PropAnim?.Duration ?? 1f;
            set { var p = PropAnim; if (p != null) p.Duration = value; }
        }
        public TweenLoop PropAnimLoop
        {
            get => PropAnim?.Loop ?? TweenLoop.Loop;
            set { var p = PropAnim; if (p != null) p.Loop = value; }
        }
        public System.Collections.Generic.IReadOnlyList<TweenLoop> PropAnimLoops { get; }
            = Scrawlbit.EnumHelper.Values<TweenLoop>();
        public bool PropAnimPlayOnStart
        {
            get => PropAnim?.PlayOnStart ?? true;
            set { var p = PropAnim; if (p != null) p.PlayOnStart = value; }
        }

        /// <summary>Trilhas em texto: uma linha por trilha "Canal: t=v, t=v" (ex.: "PositionY: 0=0, 0.5=100, 1=0").
        /// Canais: PositionX/Y, Rotation (graus), ScaleX/Y, SpriteAlpha, SpriteR/G/B.</summary>
        public string PropAnimTracksText
        {
            get
            {
                var p = PropAnim;
                if (p == null) return string.Empty;
                return string.Join("\n", p.Tracks.Select(tr =>
                    $"{tr.Channel}: " + string.Join(", ", tr.Keys.Select(k =>
                        $"{k.Time.ToString(System.Globalization.CultureInfo.InvariantCulture)}={k.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"))));
            }
            set
            {
                var p = PropAnim;
                if (p == null) return;
                p.Tracks.Clear();
                foreach (var line in (value ?? string.Empty).Split('\n'))
                {
                    var text = line.Trim();
                    if (text.Length == 0) continue;
                    int colon = text.IndexOf(':');
                    if (colon <= 0) continue;
                    if (!System.Enum.TryParse<AnimChannel>(text[..colon].Trim(), true, out var channel)) continue;

                    var track = new PropertyTrack { Channel = channel };
                    foreach (var pair in text[(colon + 1)..].Split(','))
                    {
                        var kv = pair.Split('=');
                        if (kv.Length == 2
                            && float.TryParse(kv[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var t)
                            && float.TryParse(kv[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                            track.Keys.Add(new AnimKey(t, v));
                    }
                    track.Keys.Sort((a, b) => a.Time.CompareTo(b.Time));
                    if (track.Keys.Count > 0)
                        p.Tracks.Add(track);
                }
            }
        }

        // ---- Componente Rigidbody2D (física) ----

        private Rigidbody2D? Rigidbody => _target?.Components.OfType<Rigidbody2D>().FirstOrDefault();
        public bool HasRigidbody => Rigidbody != null;

        public RigidbodyKind RigidbodyKindValue
        {
            get => Rigidbody?.Kind ?? RigidbodyKind.Dynamic;
            set { var r = Rigidbody; if (r != null) r.Kind = value; }
        }
        public System.Collections.Generic.IReadOnlyList<RigidbodyKind> RigidbodyKinds { get; }
            = Scrawlbit.EnumHelper.Values<RigidbodyKind>();

        public ColliderShape RigidbodyShapeValue
        {
            get => Rigidbody?.Shape ?? ColliderShape.Box;
            set { var r = Rigidbody; if (r != null) { r.Shape = value; OnPropertyChanged(nameof(RigidbodyIsCircle)); OnPropertyChanged(nameof(RigidbodyIsBox)); } }
        }
        public System.Collections.Generic.IReadOnlyList<ColliderShape> RigidbodyShapes { get; }
            = Scrawlbit.EnumHelper.Values<ColliderShape>();
        public bool RigidbodyIsBox => (Rigidbody?.Shape ?? ColliderShape.Box) == ColliderShape.Box;
        public bool RigidbodyIsCircle => (Rigidbody?.Shape ?? ColliderShape.Box) == ColliderShape.Circle;

        public float RigidbodyWidth { get => Rigidbody?.Width ?? 48f; set { var r = Rigidbody; if (r != null) r.Width = value; } }
        public float RigidbodyHeight { get => Rigidbody?.Height ?? 48f; set { var r = Rigidbody; if (r != null) r.Height = value; } }
        public float RigidbodyRadius { get => Rigidbody?.Radius ?? 24f; set { var r = Rigidbody; if (r != null) r.Radius = value; } }
        public float RigidbodyDensity { get => Rigidbody?.Density ?? 1f; set { var r = Rigidbody; if (r != null) r.Density = value; } }
        public float RigidbodyFriction { get => Rigidbody?.Friction ?? 0.3f; set { var r = Rigidbody; if (r != null) r.Friction = value; } }
        public float RigidbodyRestitution { get => Rigidbody?.Restitution ?? 0f; set { var r = Rigidbody; if (r != null) r.Restitution = value; } }
        public bool RigidbodyFixedRotation { get => Rigidbody?.FixedRotation ?? false; set { var r = Rigidbody; if (r != null) r.FixedRotation = value; } }
        public int RigidbodyCategory { get => Rigidbody?.CollisionCategory ?? 1; set { var r = Rigidbody; if (r != null) r.CollisionCategory = value; } }
        public int RigidbodyMask { get => Rigidbody?.CollidesWith ?? -1; set { var r = Rigidbody; if (r != null) r.CollidesWith = value; } }

        // ---- Componente TimerComponent (timer) ----

        private TimerComponent? Timer => _target?.Components.OfType<TimerComponent>().FirstOrDefault();
        public bool HasTimer => Timer != null;

        public float TimerDuration
        {
            get => Timer?.Duration ?? 1f;
            set { var t = Timer; if (t != null) t.Duration = value; }
        }
        public bool TimerRepeat
        {
            get => Timer?.Repeat ?? false;
            set { var t = Timer; if (t != null) t.Repeat = value; }
        }
        public bool TimerAutoStart
        {
            get => Timer?.AutoStart ?? true;
            set { var t = Timer; if (t != null) t.AutoStart = value; }
        }
        public string TimerSendOnElapsed
        {
            get => Timer?.SendOnElapsed ?? string.Empty;
            set { var t = Timer; if (t != null) t.SendOnElapsed = value; }
        }
        public string TimerStartOn
        {
            get => Timer?.StartOn ?? string.Empty;
            set { var t = Timer; if (t != null) t.StartOn = value; }
        }

        // ---- Componente ParallaxLayer (fundo com parallax) ----

        private ParallaxLayer? Parallax => _target?.Components.OfType<ParallaxLayer>().FirstOrDefault();
        public bool HasParallax => Parallax != null;

        public float ParallaxFactorX
        {
            get => Parallax?.FactorX ?? 0.5f;
            set { var p = Parallax; if (p != null) p.FactorX = value; }
        }
        public float ParallaxFactorY
        {
            get => Parallax?.FactorY ?? 1f;
            set { var p = Parallax; if (p != null) p.FactorY = value; }
        }

        // ---- Componente UiAnchor (âncora de HUD) ----

        private UiAnchor? Anchor => _target?.Components.OfType<UiAnchor>().FirstOrDefault();
        public bool HasAnchor => Anchor != null;

        public AnchorPoint AnchorPointValue
        {
            get => Anchor?.Anchor ?? AnchorPoint.TopLeft;
            set { var a = Anchor; if (a != null) a.Anchor = value; }
        }
        public System.Collections.Generic.IReadOnlyList<AnchorPoint> AnchorPoints { get; }
            = Scrawlbit.EnumHelper.Values<AnchorPoint>();
        public float AnchorOffsetX
        {
            get => Anchor?.OffsetX ?? 0f;
            set { var a = Anchor; if (a != null) a.OffsetX = value; }
        }
        public float AnchorOffsetY
        {
            get => Anchor?.OffsetY ?? 0f;
            set { var a = Anchor; if (a != null) a.OffsetY = value; }
        }

        // ---- Componente UiLayout (contêiner de UI) ----

        private UiLayout? Layout => _target?.Components.OfType<UiLayout>().FirstOrDefault();
        public bool HasLayout => Layout != null;

        public LayoutDirection LayoutDir
        {
            get => Layout?.Direction ?? LayoutDirection.Vertical;
            set { var l = Layout; if (l != null) l.Direction = value; }
        }
        public System.Collections.Generic.IReadOnlyList<LayoutDirection> LayoutDirections { get; }
            = Scrawlbit.EnumHelper.Values<LayoutDirection>();
        public float LayoutSpacing
        {
            get => Layout?.Spacing ?? 12f;
            set { var l = Layout; if (l != null) l.Spacing = value; }
        }

        // ---- Componente UiSlider ----

        private UiSlider? Slider => _target?.Components.OfType<UiSlider>().FirstOrDefault();
        public bool HasSlider => Slider != null;
        public float SliderValue { get => Slider?.Value ?? 1f; set { var s = Slider; if (s != null) s.Value = value; } }
        public float SliderWidth { get => Slider?.Width ?? 200f; set { var s = Slider; if (s != null) s.Width = value; } }
        public string SliderBus { get => Slider?.BusTarget ?? string.Empty; set { var s = Slider; if (s != null) s.BusTarget = value; } }
        public string SliderSendOnChange { get => Slider?.SendOnChange ?? string.Empty; set { var s = Slider; if (s != null) s.SendOnChange = value; } }

        // ---- Componente UiToggle ----

        private UiToggle? Toggle => _target?.Components.OfType<UiToggle>().FirstOrDefault();
        public bool HasToggle => Toggle != null;
        public bool ToggleIsOn { get => Toggle?.IsOn ?? false; set { var t = Toggle; if (t != null) t.IsOn = value; } }
        public float ToggleSize { get => Toggle?.Size ?? 28f; set { var t = Toggle; if (t != null) t.Size = value; } }
        public string ToggleSendOnChange { get => Toggle?.SendOnChange ?? string.Empty; set { var t = Toggle; if (t != null) t.SendOnChange = value; } }

        // ---- Componente UiProgressBar ----

        private UiProgressBar? ProgressBar => _target?.Components.OfType<UiProgressBar>().FirstOrDefault();
        public bool HasProgressBar => ProgressBar != null;
        public float ProgressValue { get => ProgressBar?.Value ?? 1f; set { var b = ProgressBar; if (b != null) b.Value = value; } }
        public float ProgressWidth { get => ProgressBar?.Width ?? 120f; set { var b = ProgressBar; if (b != null) b.Width = value; } }
        public float ProgressHeight { get => ProgressBar?.Height ?? 14f; set { var b = ProgressBar; if (b != null) b.Height = value; } }

        // ---- Componente UiTextField ----

        private UiTextField? TextField => _target?.Components.OfType<UiTextField>().FirstOrDefault();
        public bool HasTextField => TextField != null;
        public string TextFieldText { get => TextField?.Text ?? string.Empty; set { var f = TextField; if (f != null) f.Text = value; } }
        public string TextFieldPlaceholder { get => TextField?.Placeholder ?? string.Empty; set { var f = TextField; if (f != null) f.Placeholder = value; } }
        public float TextFieldWidth { get => TextField?.Width ?? 220f; set { var f = TextField; if (f != null) f.Width = value; } }
        public int TextFieldMaxLength { get => TextField?.MaxLength ?? 32; set { var f = TextField; if (f != null) f.MaxLength = value; } }
        public string TextFieldSendOnSubmit { get => TextField?.SendOnSubmit ?? string.Empty; set { var f = TextField; if (f != null) f.SendOnSubmit = value; } }

        // ---- Componente UiNavigator ----

        private UiNavigator? Navigator => _target?.Components.OfType<UiNavigator>().FirstOrDefault();
        public bool HasNavigator => Navigator != null;
        public bool NavigatorAutoFocus { get => Navigator?.AutoFocusFirst ?? true; set { var n = Navigator; if (n != null) n.AutoFocusFirst = value; } }

        // ---- Componente UiScrollView ----

        private UiScrollView? ScrollView => _target?.Components.OfType<UiScrollView>().FirstOrDefault();
        public bool HasScrollView => ScrollView != null;
        public float ScrollWidth { get => ScrollView?.Width ?? 240f; set { var s = ScrollView; if (s != null) s.Width = value; } }
        public float ScrollHeight { get => ScrollView?.Height ?? 260f; set { var s = ScrollView; if (s != null) s.Height = value; } }
        public float ScrollSpacing { get => ScrollView?.Spacing ?? 8f; set { var s = ScrollView; if (s != null) s.Spacing = value; } }
        public float ScrollSpeedValue { get => ScrollView?.ScrollSpeed ?? 24f; set { var s = ScrollView; if (s != null) s.ScrollSpeed = value; } }

        // ---- Componente UiButton (botão de UI) ----

        private UiButton? Button => _target?.Components.OfType<UiButton>().FirstOrDefault();
        public bool HasButton => Button != null;

        public float ButtonWidth
        {
            get => Button?.Width ?? 160f;
            set { var b = Button; if (b != null) b.Width = value; }
        }
        public float ButtonHeight
        {
            get => Button?.Height ?? 48f;
            set { var b = Button; if (b != null) b.Height = value; }
        }
        public string ButtonSendOnClick
        {
            get => Button?.SendOnClick ?? string.Empty;
            set { var b = Button; if (b != null) b.SendOnClick = value; }
        }
        public int ButtonR { get => Button?.Normal.R ?? 60; set { var b = Button; if (b != null) b.Normal = new Color((byte)Clamp(value), b.Normal.G, b.Normal.B); } }
        public int ButtonG { get => Button?.Normal.G ?? 70; set { var b = Button; if (b != null) b.Normal = new Color(b.Normal.R, (byte)Clamp(value), b.Normal.B); } }
        public int ButtonB { get => Button?.Normal.B ?? 90; set { var b = Button; if (b != null) b.Normal = new Color(b.Normal.R, b.Normal.G, (byte)Clamp(value)); } }

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
        public string AudioBus
        {
            get => Audio?.Bus ?? DreamBit.Engine.Audio.AudioMixer.Sfx;
            set { var a = Audio; if (a != null) a.Bus = value; }
        }
        public System.Collections.Generic.IReadOnlyList<string> AudioBuses { get; } =
            new[] { DreamBit.Engine.Audio.AudioMixer.Master, DreamBit.Engine.Audio.AudioMixer.Music, DreamBit.Engine.Audio.AudioMixer.Sfx };

        // ---- Componente TilemapRenderer ----

        private TilemapRenderer? Tilemap => _target?.Components.OfType<TilemapRenderer>().FirstOrDefault();
        public bool HasTilemap => Tilemap != null;

        public string TilemapPath
        {
            get => Tilemap?.TmxPath ?? string.Empty;
            set { var t = Tilemap; if (t != null) { t.TmxPath = string.IsNullOrWhiteSpace(value) ? null : value; Refresh(); } }
        }
        public bool TilemapSolid
        {
            get => Tilemap?.Solid ?? false;
            set { var t = Tilemap; if (t != null) t.Solid = value; }
        }
        public string TilemapSolidLayer
        {
            get => Tilemap?.SolidLayer ?? string.Empty;
            set { var t = Tilemap; if (t != null) t.SolidLayer = value; }
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

        // ---- clipes nomeados ----

        public System.Collections.Generic.IEnumerable<string> SkeletonClipNames => Skeleton?.ClipNames ?? Enumerable.Empty<string>();

        /// <summary>Clipe ativo (idle/walk…). Trocar reavalia toda a seção de skeleton.</summary>
        public string SkeletonCurrentClip
        {
            get => Skeleton?.CurrentClipName ?? string.Empty;
            set { var s = Skeleton; if (s != null && s.CurrentClipName != value) { s.CurrentClipName = value; Refresh(); } }
        }

        private string _newClipName = "walk";
        public string NewClipName
        {
            get => _newClipName;
            set { _newClipName = value; OnPropertyChanged(); }
        }

        public void AddSkeletonClip()
        {
            var s = Skeleton;
            if (s == null) return;
            s.AddClip(string.IsNullOrWhiteSpace(_newClipName) ? "clip" : _newClipName);
            Refresh();
        }

        public void RemoveSkeletonClip()
        {
            var s = Skeleton;
            if (s == null) return;
            s.RemoveClip(s.CurrentClipName);
            Refresh();
        }

        public Scrawlbit.EasingMode SkeletonEasing
        {
            get => Skeleton?.Easing ?? Scrawlbit.EasingMode.Linear;
            set { var s = Skeleton; if (s != null) s.Easing = value; }
        }
        public System.Collections.Generic.IReadOnlyList<Scrawlbit.EasingMode> EasingModes { get; }
            = Scrawlbit.EnumHelper.Values<Scrawlbit.EasingMode>();

        /// <summary>Eventos do clipe em texto: uma linha por evento "tempo: nome" (ex.: "0.5: passo").</summary>
        public string SkeletonEventsText
        {
            get
            {
                var s = Skeleton;
                if (s == null) return string.Empty;
                return string.Join("\n", s.Events.OrderBy(e => e.Time).Select(e => $"{e.Time:0.###}: {e.Name}"));
            }
            set
            {
                var s = Skeleton;
                if (s == null) return;

                var parsed = new System.Collections.Generic.List<(float, string)>();
                foreach (var raw in (value ?? string.Empty).Split('\n'))
                {
                    var line = raw.Trim();
                    int sep = line.IndexOf(':');
                    if (sep <= 0) continue;
                    if (float.TryParse(line[..sep].Trim(), System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float time))
                    {
                        var name = line[(sep + 1)..].Trim();
                        if (name.Length > 0) parsed.Add((time, name));
                    }
                }
                s.SetEvents(parsed);
                OnPropertyChanged();
            }
        }

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

        // ---- Sprite Animator Controller ----
        private SpriteAnimatorController? SAC => _target?.Components.OfType<SpriteAnimatorController>().FirstOrDefault();
        public bool HasSpriteAnimCtrl => SAC != null;
        public string SacIdleClip { get => SAC?.IdleClip ?? "idle"; set { var c = SAC; if (c != null) c.IdleClip = value; } }
        public string SacWalkClip { get => SAC?.WalkClip ?? "walk"; set { var c = SAC; if (c != null) c.WalkClip = value; } }
        public string SacJumpClip { get => SAC?.JumpClip ?? "jump"; set { var c = SAC; if (c != null) c.JumpClip = value; } }
        public string SacAttackClip { get => SAC?.AttackClip ?? "attack"; set { var c = SAC; if (c != null) c.AttackClip = value; } }
        public string SacAttackAction { get => SAC?.AttackAction ?? "Action"; set { var c = SAC; if (c != null) c.AttackAction = value; } }
        public bool SacFlipByVelocity { get => SAC?.FlipByVelocity ?? true; set { var c = SAC; if (c != null) c.FlipByVelocity = value; } }
        public bool SacArtFacesRight { get => SAC?.ArtFacesRight ?? true; set { var c = SAC; if (c != null) c.ArtFacesRight = value; } }
        public float SacBlendTime { get => SAC?.BlendTime ?? 0.1f; set { var c = SAC; if (c != null) c.BlendTime = value; } }

        // ---- Nav Chaser ----
        private NavChaser? Chaser => _target?.Components.OfType<NavChaser>().FirstOrDefault();
        public bool HasNavChaser => Chaser != null;
        public string ChaserTargetTag { get => Chaser?.TargetTag ?? "player"; set { var c = Chaser; if (c != null) c.TargetTag = value; } }
        public float ChaserSpeed { get => Chaser?.Speed ?? 120f; set { var c = Chaser; if (c != null) c.Speed = value; } }
        public float ChaserRepath { get => Chaser?.RepathInterval ?? 0.4f; set { var c = Chaser; if (c != null) c.RepathInterval = value; } }
        public float ChaserArrive { get => Chaser?.ArriveRadius ?? 10f; set { var c = Chaser; if (c != null) c.ArriveRadius = value; } }
        public bool ChaserDiagonal { get => Chaser?.AllowDiagonal ?? true; set { var c = Chaser; if (c != null) c.AllowDiagonal = value; } }

        // ---- Audio Listener ----
        public bool HasAudioListener => _target?.Components.OfType<AudioListener>().Any() ?? false;

        // ---- Light 2D ----
        private Light2D? Light => _target?.Components.OfType<Light2D>().FirstOrDefault();
        public bool HasLight => Light != null;
        public float LightRadius { get => Light?.Radius ?? 200f; set { var c = Light; if (c != null) c.Radius = value; } }
        public float LightIntensity { get => Light?.Intensity ?? 1f; set { var c = Light; if (c != null) c.Intensity = value; } }
        public int LightR { get => Light?.Color.R ?? 255; set { var c = Light; if (c != null) c.Color = new Microsoft.Xna.Framework.Color((byte)value, c.Color.G, c.Color.B); } }
        public int LightG { get => Light?.Color.G ?? 240; set { var c = Light; if (c != null) c.Color = new Microsoft.Xna.Framework.Color(c.Color.R, (byte)value, c.Color.B); } }
        public int LightB { get => Light?.Color.B ?? 210; set { var c = Light; if (c != null) c.Color = new Microsoft.Xna.Framework.Color(c.Color.R, c.Color.G, (byte)value); } }

        // ---- Ambient Light ----
        private AmbientLight? Ambient => _target?.Components.OfType<AmbientLight>().FirstOrDefault();
        public bool HasAmbient => Ambient != null;
        public int AmbientR { get => Ambient?.Color.R ?? 40; set { var c = Ambient; if (c != null) c.Color = new Microsoft.Xna.Framework.Color((byte)value, c.Color.G, c.Color.B); } }
        public int AmbientG { get => Ambient?.Color.G ?? 44; set { var c = Ambient; if (c != null) c.Color = new Microsoft.Xna.Framework.Color(c.Color.R, (byte)value, c.Color.B); } }
        public int AmbientB { get => Ambient?.Color.B ?? 60; set { var c = Ambient; if (c != null) c.Color = new Microsoft.Xna.Framework.Color(c.Color.R, c.Color.G, (byte)value); } }

        // ---- Top-Down Controller ----
        private TopDownController? TopDown => _target?.Components.OfType<TopDownController>().FirstOrDefault();
        public bool HasTopDown => TopDown != null;
        public float TopDownSpeed { get => TopDown?.MoveSpeed ?? 200f; set { var c = TopDown; if (c != null) c.MoveSpeed = value; } }
        public float TopDownHalfWidth { get => TopDown?.HalfWidth ?? 16f; set { var c = TopDown; if (c != null) c.HalfWidth = value; } }
        public float TopDownHalfHeight { get => TopDown?.HalfHeight ?? 16f; set { var c = TopDown; if (c != null) c.HalfHeight = value; } }
        public bool TopDownUseKeyboard { get => TopDown?.UseKeyboard ?? true; set { var c = TopDown; if (c != null) c.UseKeyboard = value; } }

        // ---- Health ----
        private Health? HealthC => _target?.Components.OfType<Health>().FirstOrDefault();
        public bool HasHealth => HealthC != null;
        public float HealthMax { get => HealthC?.Max ?? 100f; set { var c = HealthC; if (c != null) c.Max = value; } }
        public float HealthInvuln { get => HealthC?.InvulnTime ?? 0.2f; set { var c = HealthC; if (c != null) c.InvulnTime = value; } }
        public string HealthSendOnHit { get => HealthC?.SendOnHit ?? ""; set { var c = HealthC; if (c != null) c.SendOnHit = value; } }
        public string HealthSendOnDeath { get => HealthC?.SendOnDeath ?? "death"; set { var c = HealthC; if (c != null) c.SendOnDeath = value; } }
        public bool HealthDestroyOnDeath { get => HealthC?.DestroyOnDeath ?? false; set { var c = HealthC; if (c != null) c.DestroyOnDeath = value; } }

        // ---- Hurtbox ----
        private Hurtbox? Hurt => _target?.Components.OfType<Hurtbox>().FirstOrDefault();
        public bool HasHurtbox => Hurt != null;
        public float HurtWidth { get => Hurt?.Width ?? 48f; set { var c = Hurt; if (c != null) c.Width = value; } }
        public float HurtHeight { get => Hurt?.Height ?? 48f; set { var c = Hurt; if (c != null) c.Height = value; } }
        public float HurtOffsetX { get => Hurt?.Offset.X ?? 0f; set { var c = Hurt; if (c != null) c.Offset = new Microsoft.Xna.Framework.Vector2(value, c.Offset.Y); } }
        public float HurtOffsetY { get => Hurt?.Offset.Y ?? 0f; set { var c = Hurt; if (c != null) c.Offset = new Microsoft.Xna.Framework.Vector2(c.Offset.X, value); } }
        public int HurtTeam { get => Hurt?.Team ?? 0; set { var c = Hurt; if (c != null) c.Team = value; } }

        // ---- Hitbox ----
        private Hitbox? Hit => _target?.Components.OfType<Hitbox>().FirstOrDefault();
        public bool HasHitbox => Hit != null;
        public float HitWidth { get => Hit?.Width ?? 60f; set { var c = Hit; if (c != null) c.Width = value; } }
        public float HitHeight { get => Hit?.Height ?? 60f; set { var c = Hit; if (c != null) c.Height = value; } }
        public float HitOffsetX { get => Hit?.Offset.X ?? 0f; set { var c = Hit; if (c != null) c.Offset = new Microsoft.Xna.Framework.Vector2(value, c.Offset.Y); } }
        public float HitOffsetY { get => Hit?.Offset.Y ?? 0f; set { var c = Hit; if (c != null) c.Offset = new Microsoft.Xna.Framework.Vector2(c.Offset.X, value); } }
        public int HitTeam { get => Hit?.Team ?? 0; set { var c = Hit; if (c != null) c.Team = value; } }
        public float HitDamage { get => Hit?.Damage ?? 25f; set { var c = Hit; if (c != null) c.Damage = value; } }
        public float HitActiveTime { get => Hit?.ActiveTime ?? 0.15f; set { var c = Hit; if (c != null) c.ActiveTime = value; } }
        public string HitActivateOn { get => Hit?.ActivateOn ?? ""; set { var c = Hit; if (c != null) c.ActivateOn = value; } }

        // ---- Sprite Flash ----
        private SpriteFlash? Flash => _target?.Components.OfType<SpriteFlash>().FirstOrDefault();
        public bool HasSpriteFlash => Flash != null;
        public float FlashDuration { get => Flash?.Duration ?? 0.1f; set { var c = Flash; if (c != null) c.Duration = value; } }
        public int FlashR { get => Flash?.FlashColor.R ?? 255; set { var c = Flash; if (c != null) c.FlashColor = new Microsoft.Xna.Framework.Color((byte)value, c.FlashColor.G, c.FlashColor.B); } }
        public int FlashG { get => Flash?.FlashColor.G ?? 255; set { var c = Flash; if (c != null) c.FlashColor = new Microsoft.Xna.Framework.Color(c.FlashColor.R, (byte)value, c.FlashColor.B); } }
        public int FlashB { get => Flash?.FlashColor.B ?? 255; set { var c = Flash; if (c != null) c.FlashColor = new Microsoft.Xna.Framework.Color(c.FlashColor.R, c.FlashColor.G, (byte)value); } }

        // ---- Joint 2D ----
        private Joint2D? JointC => _target?.Components.OfType<Joint2D>().FirstOrDefault();
        public bool HasJoint => JointC != null;
        public System.Collections.Generic.IReadOnlyList<Joint2DKind> JointKinds { get; } = Scrawlbit.EnumHelper.Values<Joint2DKind>();
        public Joint2DKind JointKindValue { get => JointC?.Kind ?? Joint2DKind.Distance; set { var c = JointC; if (c != null) c.Kind = value; } }
        public string JointConnectedTag { get => JointC?.ConnectedTag ?? ""; set { var c = JointC; if (c != null) c.ConnectedTag = value; } }
        public float JointAnchorX { get => JointC?.Anchor.X ?? 0f; set { var c = JointC; if (c != null) c.Anchor = new Microsoft.Xna.Framework.Vector2(value, c.Anchor.Y); } }
        public float JointAnchorY { get => JointC?.Anchor.Y ?? 0f; set { var c = JointC; if (c != null) c.Anchor = new Microsoft.Xna.Framework.Vector2(c.Anchor.X, value); } }
        public float JointFrequency { get => JointC?.Frequency ?? 0f; set { var c = JointC; if (c != null) c.Frequency = value; } }
        public float JointDamping { get => JointC?.DampingRatio ?? 0.5f; set { var c = JointC; if (c != null) c.DampingRatio = value; } }
        public bool JointCollideConnected { get => JointC?.CollideConnected ?? false; set { var c = JointC; if (c != null) c.CollideConnected = value; } }

        // ---- Shadow Caster ----
        private ShadowCaster? Shadow => _target?.Components.OfType<ShadowCaster>().FirstOrDefault();
        public bool HasShadowCaster => Shadow != null;
        public float ShadowWidth { get => Shadow?.Width ?? 64f; set { var c = Shadow; if (c != null) c.Width = value; } }
        public float ShadowHeight { get => Shadow?.Height ?? 64f; set { var c = Shadow; if (c != null) c.Height = value; } }
        public float ShadowOffsetX { get => Shadow?.Offset.X ?? 0f; set { var c = Shadow; if (c != null) c.Offset = new Microsoft.Xna.Framework.Vector2(value, c.Offset.Y); } }
        public float ShadowOffsetY { get => Shadow?.Offset.Y ?? 0f; set { var c = Shadow; if (c != null) c.Offset = new Microsoft.Xna.Framework.Vector2(c.Offset.X, value); } }

        // ---- Y Sort ----
        private YSort? YSortC => _target?.Components.OfType<YSort>().FirstOrDefault();
        public bool HasYSort => YSortC != null;
        public float YSortOffset { get => YSortC?.Offset ?? 0f; set { var c = YSortC; if (c != null) c.Offset = value; } }

        // ---- Tilemap orientação (isométrico) ----
        public System.Collections.Generic.IReadOnlyList<TileOrientation> TileOrientations { get; } = Scrawlbit.EnumHelper.Values<TileOrientation>();
        public TileOrientation TilemapOrientation
        {
            get => _target?.Components.OfType<TilemapRenderer>().FirstOrDefault()?.Orientation ?? TileOrientation.Orthogonal;
            set { var c = _target?.Components.OfType<TilemapRenderer>().FirstOrDefault(); if (c != null) c.Orientation = value; }
        }

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(HasTarget));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Tag));
            OnPropertyChanged(nameof(SortOrder));
            OnPropertyChanged(nameof(RenderLayer));
            OnPropertyChanged(nameof(ScreenSpace));
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
            OnPropertyChanged(nameof(SpriteChroma));
            OnPropertyChanged(nameof(SpriteChromaAuto));
            OnPropertyChanged(nameof(SpriteChromaR));
            OnPropertyChanged(nameof(SpriteChromaG));
            OnPropertyChanged(nameof(SpriteChromaB));
            OnPropertyChanged(nameof(SpriteChromaTolerance));
            OnPropertyChanged(nameof(AnimChroma));
            OnPropertyChanged(nameof(AnimChromaAuto));
            OnPropertyChanged(nameof(AnimChromaR));
            OnPropertyChanged(nameof(AnimChromaG));
            OnPropertyChanged(nameof(AnimChromaB));
            OnPropertyChanged(nameof(AnimChromaTolerance));
            OnPropertyChanged(nameof(HasScript));
            OnPropertyChanged(nameof(ScriptSource));
            OnPropertyChanged(nameof(ScriptSourcePath));
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
            OnPropertyChanged(nameof(HasAnimController));
            OnPropertyChanged(nameof(CtrlIdleClip));
            OnPropertyChanged(nameof(CtrlWalkClip));
            OnPropertyChanged(nameof(CtrlJumpClip)); OnPropertyChanged(nameof(CtrlBlendTime));
            OnPropertyChanged(nameof(HasTween));
            OnPropertyChanged(nameof(TweenChannel));
            OnPropertyChanged(nameof(TweenLoop));
            OnPropertyChanged(nameof(TweenEasing));
            OnPropertyChanged(nameof(TweenFrom));
            OnPropertyChanged(nameof(TweenTo));
            OnPropertyChanged(nameof(TweenDuration));
            OnPropertyChanged(nameof(TweenPlayOnStart));
            OnPropertyChanged(nameof(HasText));
            OnPropertyChanged(nameof(TextValue));
            OnPropertyChanged(nameof(TextLocKey));
            OnPropertyChanged(nameof(TextPixelSize));
            OnPropertyChanged(nameof(TextScreenSpace));
            OnPropertyChanged(nameof(TextR));
            OnPropertyChanged(nameof(TextG));
            OnPropertyChanged(nameof(TextB));
            OnPropertyChanged(nameof(HasCamera));
            OnPropertyChanged(nameof(CameraTargetTag));
            OnPropertyChanged(nameof(CameraDeadzoneWidth));
            OnPropertyChanged(nameof(CameraDeadzoneHeight));
            OnPropertyChanged(nameof(CameraSmoothTime));
            OnPropertyChanged(nameof(CameraZoom));
            OnPropertyChanged(nameof(CameraUseBounds));
            OnPropertyChanged(nameof(CameraPriority)); OnPropertyChanged(nameof(CameraShakeOn));
            OnPropertyChanged(nameof(CameraShakeDuration)); OnPropertyChanged(nameof(CameraShakeMagnitude));
            OnPropertyChanged(nameof(HasPropAnim));
            OnPropertyChanged(nameof(PropAnimDuration));
            OnPropertyChanged(nameof(PropAnimLoop));
            OnPropertyChanged(nameof(PropAnimPlayOnStart));
            OnPropertyChanged(nameof(PropAnimTracksText));
            OnPropertyChanged(nameof(HasRigidbody));
            OnPropertyChanged(nameof(RigidbodyKindValue));
            OnPropertyChanged(nameof(RigidbodyShapeValue));
            OnPropertyChanged(nameof(RigidbodyIsBox));
            OnPropertyChanged(nameof(RigidbodyIsCircle));
            OnPropertyChanged(nameof(RigidbodyWidth));
            OnPropertyChanged(nameof(RigidbodyHeight));
            OnPropertyChanged(nameof(RigidbodyRadius));
            OnPropertyChanged(nameof(RigidbodyDensity));
            OnPropertyChanged(nameof(RigidbodyFriction));
            OnPropertyChanged(nameof(RigidbodyRestitution));
            OnPropertyChanged(nameof(RigidbodyFixedRotation));
            OnPropertyChanged(nameof(RigidbodyCategory));
            OnPropertyChanged(nameof(RigidbodyMask));
            OnPropertyChanged(nameof(HasTimer));
            OnPropertyChanged(nameof(TimerDuration));
            OnPropertyChanged(nameof(TimerRepeat));
            OnPropertyChanged(nameof(TimerAutoStart));
            OnPropertyChanged(nameof(TimerSendOnElapsed));
            OnPropertyChanged(nameof(TimerStartOn));
            OnPropertyChanged(nameof(HasParallax));
            OnPropertyChanged(nameof(ParallaxFactorX));
            OnPropertyChanged(nameof(ParallaxFactorY));
            OnPropertyChanged(nameof(HasLayout));
            OnPropertyChanged(nameof(LayoutDir));
            OnPropertyChanged(nameof(LayoutSpacing));
            OnPropertyChanged(nameof(HasSlider));
            OnPropertyChanged(nameof(SliderValue));
            OnPropertyChanged(nameof(SliderWidth));
            OnPropertyChanged(nameof(SliderBus));
            OnPropertyChanged(nameof(SliderSendOnChange));
            OnPropertyChanged(nameof(HasToggle));
            OnPropertyChanged(nameof(ToggleIsOn));
            OnPropertyChanged(nameof(ToggleSize));
            OnPropertyChanged(nameof(ToggleSendOnChange));
            OnPropertyChanged(nameof(HasProgressBar));
            OnPropertyChanged(nameof(ProgressValue));
            OnPropertyChanged(nameof(ProgressWidth));
            OnPropertyChanged(nameof(ProgressHeight));
            OnPropertyChanged(nameof(HasTextField));
            OnPropertyChanged(nameof(TextFieldText));
            OnPropertyChanged(nameof(TextFieldPlaceholder));
            OnPropertyChanged(nameof(TextFieldWidth));
            OnPropertyChanged(nameof(TextFieldMaxLength));
            OnPropertyChanged(nameof(TextFieldSendOnSubmit));
            OnPropertyChanged(nameof(HasNavigator));
            OnPropertyChanged(nameof(NavigatorAutoFocus));
            OnPropertyChanged(nameof(HasScrollView));
            OnPropertyChanged(nameof(ScrollWidth));
            OnPropertyChanged(nameof(ScrollHeight));
            OnPropertyChanged(nameof(ScrollSpacing));
            OnPropertyChanged(nameof(ScrollSpeedValue));
            OnPropertyChanged(nameof(HasAnchor));
            OnPropertyChanged(nameof(AnchorPointValue));
            OnPropertyChanged(nameof(AnchorOffsetX));
            OnPropertyChanged(nameof(AnchorOffsetY));
            OnPropertyChanged(nameof(HasButton));
            OnPropertyChanged(nameof(ButtonWidth));
            OnPropertyChanged(nameof(ButtonHeight));
            OnPropertyChanged(nameof(ButtonSendOnClick));
            OnPropertyChanged(nameof(ButtonR));
            OnPropertyChanged(nameof(ButtonG));
            OnPropertyChanged(nameof(ButtonB));
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
            OnPropertyChanged(nameof(AudioBus));
            OnPropertyChanged(nameof(HasTilemap));
            OnPropertyChanged(nameof(TilemapPath));
            OnPropertyChanged(nameof(TilemapSolid));
            OnPropertyChanged(nameof(TilemapSolidLayer));
            OnPropertyChanged(nameof(TilemapPath));
            OnPropertyChanged(nameof(HasPlatformer));
            OnPropertyChanged(nameof(PlatGravity));
            OnPropertyChanged(nameof(PlatHalfHeight));
            OnPropertyChanged(nameof(PlatHalfWidth));
            OnPropertyChanged(nameof(HasCollider));
            OnPropertyChanged(nameof(ColliderWidth));
            OnPropertyChanged(nameof(ColliderHeight));
            OnPropertyChanged(nameof(ColliderOffsetX));
            OnPropertyChanged(nameof(ColliderOffsetY));
            OnPropertyChanged(nameof(PlatHorizontalSpeed));
            OnPropertyChanged(nameof(PlatUseKeyboard));
            OnPropertyChanged(nameof(PlatMoveSpeed));
            OnPropertyChanged(nameof(PlatJumpSpeed));
            OnPropertyChanged(nameof(HasAnimator));
            OnPropertyChanged(nameof(AnimTexturePath));
            OnPropertyChanged(nameof(AnimFrameWidth));
            OnPropertyChanged(nameof(AnimFrameHeight));
            OnPropertyChanged(nameof(AnimFrameCount));
            OnPropertyChanged(nameof(AnimDetectedFrames));
            OnPropertyChanged(nameof(AnimUsesDetectedFrames));
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
            OnPropertyChanged(nameof(SkeletonEasing));
            OnPropertyChanged(nameof(SkeletonEventsText));
            OnPropertyChanged(nameof(SkeletonClipNames));
            OnPropertyChanged(nameof(SkeletonCurrentClip));

            OnPropertyChanged(nameof(HasSpriteAnimCtrl));
            OnPropertyChanged(nameof(SacIdleClip)); OnPropertyChanged(nameof(SacWalkClip));
            OnPropertyChanged(nameof(SacJumpClip)); OnPropertyChanged(nameof(SacAttackClip));
            OnPropertyChanged(nameof(SacAttackAction)); OnPropertyChanged(nameof(SacFlipByVelocity));
            OnPropertyChanged(nameof(SacArtFacesRight)); OnPropertyChanged(nameof(SacBlendTime));
            OnPropertyChanged(nameof(HasNavChaser));
            OnPropertyChanged(nameof(ChaserTargetTag)); OnPropertyChanged(nameof(ChaserSpeed));
            OnPropertyChanged(nameof(ChaserRepath)); OnPropertyChanged(nameof(ChaserArrive));
            OnPropertyChanged(nameof(ChaserDiagonal));
            OnPropertyChanged(nameof(HasAudioListener));
            OnPropertyChanged(nameof(HasLight));
            OnPropertyChanged(nameof(LightRadius)); OnPropertyChanged(nameof(LightIntensity));
            OnPropertyChanged(nameof(LightR)); OnPropertyChanged(nameof(LightG)); OnPropertyChanged(nameof(LightB));
            OnPropertyChanged(nameof(HasAmbient));
            OnPropertyChanged(nameof(AmbientR)); OnPropertyChanged(nameof(AmbientG)); OnPropertyChanged(nameof(AmbientB));
            OnPropertyChanged(nameof(HasTopDown));
            OnPropertyChanged(nameof(TopDownSpeed)); OnPropertyChanged(nameof(TopDownHalfWidth));
            OnPropertyChanged(nameof(TopDownHalfHeight)); OnPropertyChanged(nameof(TopDownUseKeyboard));
            OnPropertyChanged(nameof(HasHealth));
            OnPropertyChanged(nameof(HealthMax)); OnPropertyChanged(nameof(HealthInvuln));
            OnPropertyChanged(nameof(HealthSendOnHit)); OnPropertyChanged(nameof(HealthSendOnDeath));
            OnPropertyChanged(nameof(HealthDestroyOnDeath));
            OnPropertyChanged(nameof(HasHurtbox));
            OnPropertyChanged(nameof(HurtWidth)); OnPropertyChanged(nameof(HurtHeight));
            OnPropertyChanged(nameof(HurtOffsetX)); OnPropertyChanged(nameof(HurtOffsetY)); OnPropertyChanged(nameof(HurtTeam));
            OnPropertyChanged(nameof(HasHitbox));
            OnPropertyChanged(nameof(HitWidth)); OnPropertyChanged(nameof(HitHeight));
            OnPropertyChanged(nameof(HitOffsetX)); OnPropertyChanged(nameof(HitOffsetY)); OnPropertyChanged(nameof(HitTeam));
            OnPropertyChanged(nameof(HitDamage)); OnPropertyChanged(nameof(HitActiveTime)); OnPropertyChanged(nameof(HitActivateOn));
            OnPropertyChanged(nameof(HasSpriteFlash));
            OnPropertyChanged(nameof(FlashDuration));
            OnPropertyChanged(nameof(FlashR)); OnPropertyChanged(nameof(FlashG)); OnPropertyChanged(nameof(FlashB));
            OnPropertyChanged(nameof(HasJoint));
            OnPropertyChanged(nameof(JointKindValue)); OnPropertyChanged(nameof(JointConnectedTag));
            OnPropertyChanged(nameof(JointAnchorX)); OnPropertyChanged(nameof(JointAnchorY));
            OnPropertyChanged(nameof(JointFrequency)); OnPropertyChanged(nameof(JointDamping));
            OnPropertyChanged(nameof(JointCollideConnected));
            OnPropertyChanged(nameof(HasShadowCaster));
            OnPropertyChanged(nameof(ShadowWidth)); OnPropertyChanged(nameof(ShadowHeight));
            OnPropertyChanged(nameof(ShadowOffsetX)); OnPropertyChanged(nameof(ShadowOffsetY));
            OnPropertyChanged(nameof(HasYSort)); OnPropertyChanged(nameof(YSortOffset));
            OnPropertyChanged(nameof(TilemapOrientation));
        }
    }
}
