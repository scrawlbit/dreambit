using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Engine.Components
{
    /// <summary>Evento disparado ao a animação entrar em um frame específico.</summary>
    public readonly record struct AnimationFrameEvent(int Frame, string Name);

    /// <summary>
    /// Anima frames de uma sprite sheet (grade de frames de tamanho fixo). No play mode
    /// avança os frames pela taxa (FPS), com repetição opcional.
    /// Pode disparar eventos nomeados ao entrar em frames específicos (<see cref="AnimationEvent"/>).
    /// </summary>
    public sealed class SpriteAnimator : SceneComponent
    {
        private string? _texturePath;
        private int _frameWidth = 32;
        private int _frameHeight = 32;
        private int _frameCount = 1;
        private float _fps = 8f;
        private bool _loop = true;
        private Vector2 _size = new(64, 64);
        private bool _chromaKey;
        private bool _chromaAuto = true;
        private Color _chromaColor = new(255, 0, 255);
        private int _chromaTolerance = 30;

        private int _currentFrame;
        private double _accumulator;
        private bool _flipX;
        private readonly Dictionary<int, string> _frameEvents = new();
        private readonly List<Rectangle> _frames = new();
        private readonly List<SpriteClip> _clips = new();
        private SpriteClip? _activeClip;
        private bool _clipDone;

        // Crossfade (dissolve) entre clipes: frame de saída congelado + tempo do blend.
        private int _fadeFrame = -1;
        private float _fadeTime;
        private float _fadeElapsed;

        public override string DisplayName => "Sprite Animator";

        /// <summary>Espelha o sprite na horizontal (para virar o personagem ao mudar de direção).</summary>
        public bool FlipX { get => _flipX; set => Set(ref _flipX, value); }

        /// <summary>Cor multiplicada no desenho (branco = sem efeito). Usada pelo <see cref="SpriteFlash"/>.</summary>
        public Color Tint { get; set; } = Color.White;

        /// <summary>Clipes nomeados (andar/pular/bater). Vazio = toca a folha inteira como hoje.</summary>
        public IReadOnlyList<SpriteClip> Clips => _clips;

        /// <summary>Nome do clipe em reprodução, ou null (folha inteira).</summary>
        public string? CurrentClip => _activeClip?.Name;

        /// <summary>True quando o clipe ativo (sem repetição) já chegou ao fim.</summary>
        public bool CurrentClipFinished => _clipDone;

        /// <summary>Disparado quando um clipe sem repetição chega ao fim (para encadear estados,
        /// ex.: voltar ao idle depois do ataque). Passa o nome do clipe.</summary>
        public event Action<string>? ClipFinished;

        /// <summary>Substitui o conjunto de clipes.</summary>
        public void SetClips(IEnumerable<SpriteClip> clips)
        {
            _clips.Clear();
            if (clips != null)
                _clips.AddRange(clips);
            _activeClip = null;
            OnPropertyChanged(nameof(Clips));
        }

        /// <summary>Adiciona (ou substitui pelo nome) um clipe.</summary>
        public void AddClip(SpriteClip clip)
        {
            if (clip == null) return;
            _clips.RemoveAll(c => c.Name == clip.Name);
            _clips.Add(clip);
            OnPropertyChanged(nameof(Clips));
        }

        /// <summary>Começa a tocar um clipe pelo nome (reinicia do primeiro frame do clipe).
        /// Nome vazio/desconhecido volta a tocar a folha inteira. Com <paramref name="blendTime"/>
        /// &gt; 0, faz um dissolve (crossfade) do frame atual para o novo clipe.</summary>
        public void Play(string? name, float blendTime = 0f)
        {
            var clip = string.IsNullOrEmpty(name) ? null : _clips.FirstOrDefault(c => c.Name == name);
            if (ReferenceEquals(clip, _activeClip) && clip != null)
                return; // já tocando este clipe

            if (blendTime > 0f)
            {
                _fadeFrame = GlobalFrame(_currentFrame); // congela o frame de saída
                _fadeTime = blendTime;
                _fadeElapsed = 0f;
            }
            else
            {
                _fadeFrame = -1;
            }

            _activeClip = clip;
            _currentFrame = 0;
            _accumulator = 0;
            _clipDone = false;
        }

        /// <summary>Peso do crossfade em andamento (0 = começando, 1 = concluído; 1 se sem blend).</summary>
        public float BlendWeight => _fadeFrame >= 0 && _fadeTime > 0f
            ? System.Math.Clamp(_fadeElapsed / _fadeTime, 0f, 1f) : 1f;

        /// <summary>Retângulos de origem explícitos (frames de tamanhos diferentes). Quando não
        /// vazio, têm prioridade sobre a grade uniforme — preenchidos pela autodetecção.</summary>
        public IReadOnlyList<Rectangle> Frames => _frames;

        /// <summary>Define os frames explícitos (substitui). Vazio volta para a grade uniforme.</summary>
        public void SetFrames(IEnumerable<Rectangle> frames)
        {
            _frames.Clear();
            if (frames != null)
                _frames.AddRange(frames);
            _currentFrame = 0;
            _accumulator = 0;
            OnPropertyChanged(nameof(Frames));
            OnPropertyChanged(nameof(EffectiveFrameCount));
        }

        /// <summary>Número de frames em uso: os explícitos, se houver, senão <see cref="FrameCount"/>.</summary>
        public int EffectiveFrameCount => _frames.Count > 0 ? _frames.Count : _frameCount;

        /// <summary>Disparado quando a animação entra em um frame que tem um evento associado.</summary>
        public event Action<string>? AnimationEvent;

        /// <summary>Eventos por frame (frame → nome). Editar via <see cref="SetEvents"/>.</summary>
        public IEnumerable<AnimationFrameEvent> Events =>
            _frameEvents.Select(kv => new AnimationFrameEvent(kv.Key, kv.Value));

        /// <summary>Define o conjunto de eventos por frame (substitui o atual).</summary>
        public void SetEvents(IEnumerable<AnimationFrameEvent> events)
        {
            _frameEvents.Clear();
            foreach (var e in events)
                if (e.Frame >= 0 && !string.IsNullOrWhiteSpace(e.Name))
                    _frameEvents[e.Frame] = e.Name.Trim();
            OnPropertyChanged(nameof(Events));
        }

        public string? TexturePath { get => _texturePath; set => Set(ref _texturePath, value); }
        public int FrameWidth { get => _frameWidth; set => Set(ref _frameWidth, Math.Max(1, value)); }
        public int FrameHeight { get => _frameHeight; set => Set(ref _frameHeight, Math.Max(1, value)); }
        public int FrameCount { get => _frameCount; set => Set(ref _frameCount, Math.Max(1, value)); }
        public float Fps { get => _fps; set => Set(ref _fps, Math.Max(0f, value)); }
        public bool Loop { get => _loop; set => Set(ref _loop, value); }
        public Vector2 Size { get => _size; set => Set(ref _size, value); }

        /// <summary>Remove a cor de fundo da sheet (chroma key).</summary>
        public bool ChromaKeyEnabled { get => _chromaKey; set => Set(ref _chromaKey, value); }
        public bool ChromaAuto { get => _chromaAuto; set => Set(ref _chromaAuto, value); }
        public Color ChromaColor { get => _chromaColor; set => Set(ref _chromaColor, value); }
        public int ChromaTolerance { get => _chromaTolerance; set => Set(ref _chromaTolerance, value < 0 ? 0 : value); }

        /// <summary>Índice na sequência ativa (dentro do clipe, se houver clipe ativo).</summary>
        public int CurrentFrame => _currentFrame;

        // Sequência ativa: o clipe corrente, senão a folha inteira.
        private int SequenceLength => _activeClip != null ? _activeClip.Frames.Length : EffectiveFrameCount;
        private float SequenceFps => _activeClip?.Fps ?? _fps;
        private bool SequenceLoop => _activeClip?.Loop ?? _loop;

        /// <summary>Traduz o índice na sequência (clipe) para o índice de frame global da folha.</summary>
        private int GlobalFrame(int sequenceIndex)
        {
            if (_activeClip == null)
                return sequenceIndex;
            if (_activeClip.Frames.Length == 0)
                return 0;
            return _activeClip.Frames[Math.Clamp(sequenceIndex, 0, _activeClip.Frames.Length - 1)];
        }

        public void Reset()
        {
            _currentFrame = 0;
            _accumulator = 0;
            _clipDone = false;
        }

        /// <summary>Avança a animação por um intervalo de tempo (lógica pura, testável).</summary>
        public void Advance(double deltaSeconds)
        {
            if (_fadeFrame >= 0)
            {
                _fadeElapsed += (float)deltaSeconds;
                if (_fadeElapsed >= _fadeTime)
                    _fadeFrame = -1; // dissolve concluído
            }

            int total = SequenceLength;
            float fps = SequenceFps;
            if (fps <= 0f || total <= 1)
                return;

            _accumulator += deltaSeconds;
            double frameTime = 1.0 / fps;

            while (_accumulator >= frameTime)
            {
                _accumulator -= frameTime;
                _currentFrame++;

                if (_currentFrame >= total)
                {
                    if (SequenceLoop)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        // Segura no último frame (já disparado ao entrar nele); não re-dispara.
                        _currentFrame = total - 1;
                        _accumulator = 0;
                        if (!_clipDone)
                        {
                            _clipDone = true;
                            if (_activeClip != null)
                                ClipFinished?.Invoke(_activeClip.Name);
                        }
                        break;
                    }
                }

                FireFrameEvent(GlobalFrame(_currentFrame)); // dispara ao entrar no frame (global)
            }
        }

        private void FireFrameEvent(int frame)
        {
            if (!_frameEvents.TryGetValue(frame, out var name))
                return;

            AnimationEvent?.Invoke(name);
            Owner?.Scene?.Send(name, Owner); // encaminha ao barramento de eventos da cena
        }

        protected internal override void Update(GameTime gameTime)
            => Advance(gameTime.ElapsedGameTime.TotalSeconds);

        protected internal override void Draw(ISceneDrawing drawing)
        {
            var device = drawing.SpriteBatch.GraphicsDevice;
            var texture = _chromaKey
                ? TextureCache.GetChromaKeyed(device, _texturePath, _chromaAuto, _chromaColor, _chromaTolerance)
                : TextureCache.Get(device, _texturePath);
            if (texture == null)
            {
                drawing.DrawQuad(Owner.Transform.WorldMatrix, _size, new Color(110, 110, 120));
                return;
            }

            var effects = _flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float w = BlendWeight; // 1 = sem dissolve

            // Frame de saída (dissolve): desenha por baixo, sumindo (alpha 1-w).
            if (_fadeFrame >= 0 && w < 1f)
                DrawFrameAt(drawing, texture, _fadeFrame, Tint * (1f - w), effects);

            // Frame atual (entrando com alpha w durante o dissolve).
            DrawFrameAt(drawing, texture, GlobalFrame(_currentFrame), Tint * w, effects);
        }

        private void DrawFrameAt(ISceneDrawing drawing, Texture2D texture, int globalFrame, Color color, SpriteEffects effects)
        {
            Rectangle source;
            if (_frames.Count > 0)
            {
                source = _frames[Math.Clamp(globalFrame, 0, _frames.Count - 1)];
            }
            else
            {
                int columns = Math.Max(1, texture.Width / _frameWidth);
                int frame = Math.Clamp(globalFrame, 0, Math.Max(0, _frameCount - 1));
                int col = frame % columns;
                int row = frame / columns;
                source = new Rectangle(col * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight);
            }

            // Mantém a proporção do frame (frames de larguras diferentes): escala pela altura.
            var drawSize = _size;
            if (source.Height > 0)
                drawSize = new Vector2(_size.Y * source.Width / source.Height, _size.Y);

            drawing.DrawFrame(Owner.Transform.WorldMatrix, drawSize, color, texture, source, effects);
        }
    }
}
