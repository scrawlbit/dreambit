using System;
using System.Collections.Generic;
using System.IO;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamBit.Engine.Hosting
{
    /// <summary>
    /// Runtime de uma cena DreamBit, independente de <see cref="Game"/>: carrega cenas (.dbscene),
    /// roda Update/Draw, conduz a câmera, aplica pós-processamento, resolução virtual com
    /// letterbox e transições pedidas por <see cref="SceneExit"/>. É o que o
    /// <see cref="DreamBitGame"/> e o DreamBit.Player usam; um jogo MonoGame próprio pode
    /// usá-lo diretamente.
    /// </summary>
    public sealed class SceneHost : IDisposable
    {
        private readonly SceneRenderer _renderer = new();
        private GraphicsDevice? _device;
        private Effect? _postEffect;
        private SpriteBatch? _batch;
        private RenderTarget2D? _target;
        private string? _sceneFolder;
        private float _fps;

        /// <summary>Cena em execução.</summary>
        public Scene Scene { get; private set; } = new();

        /// <summary>Câmera usada no desenho (conduzida pelo <see cref="CameraComponent"/> ativo).</summary>
        public Camera2D Camera { get; } = new();

        /// <summary>Renderer da cena (cor de fundo, overlay de debug etc.).</summary>
        public SceneRenderer Renderer => _renderer;

        /// <summary>Resolução lógica do jogo. Quando definida, a cena é desenhada nesse tamanho e
        /// escalada para a janela com letterbox (tela cheia, qualquer monitor). Null = tamanho
        /// da janela.</summary>
        public Point? VirtualResolution { get; set; }

        /// <summary>Cor das barras do letterbox.</summary>
        public Color LetterboxColor { get; set; } = Color.Black;

        /// <summary>Filtro ao escalar a resolução virtual (Linear para arte pintada, Point para pixel art).</summary>
        public SamplerState UpscaleSampler { get; set; } = SamplerState.LinearClamp;

        /// <summary>Se a tecla F3 alterna o overlay de debug.</summary>
        public bool DebugOverlayHotkey { get; set; } = true;

        /// <summary>Resolve o arquivo de uma transição de cena (padrão: relativo à cena atual).</summary>
        public Func<string, string?>? SceneFileResolver { get; set; }

        /// <summary>Disparado depois de uma cena ser carregada e iniciada (StartPlay).</summary>
        public event Action<Scene>? SceneLoaded;

        /// <summary>Disparado antes de trocar de cena (a cena antiga ainda está ativa).</summary>
        public event Action<Scene>? SceneUnloading;

        /// <summary>Retângulo (em pixels da janela) onde a cena virtual é apresentada.</summary>
        public Rectangle PresentRect { get; private set; }

        /// <summary>Inicializa os recursos gráficos. <paramref name="content"/> é opcional e só
        /// serve para carregar o shader de pós-processamento ("PostProcess"), se compilado.</summary>
        public void Initialize(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager? content = null)
        {
            _device = device;
            _renderer.Initialize(device);
            _renderer.ShowGrid = false;
            _renderer.ShowLedges = false;
            _batch = new SpriteBatch(device);
            if (content != null)
            {
                try { _postEffect = content.Load<Effect>("PostProcess"); }
                catch { _postEffect = null; /* sem shader compilado: renderiza normal */ }
            }
        }

        /// <summary>Carrega uma cena de arquivo e a coloca em execução.</summary>
        public Scene LoadScene(string path)
        {
            Levels.WorldNavigator.CurrentSceneFile = path;
            var full = Assets.AssetPaths.Resolve(path);
            var scene = SceneSerializer.Load(full);
            _sceneFolder = Path.GetDirectoryName(full);
            SetScene(scene);
            return scene;
        }

        /// <summary>Coloca uma cena já montada (em código ou carregada) em execução.</summary>
        public void SetScene(Scene scene)
        {
            if (Scene.Objects.Count > 0 || Scene.Ledges.Count > 0)
            {
                SceneUnloading?.Invoke(Scene);
                Levels.WorldNavigator.BeforeSceneSwap(Scene); // leva os objetos Persistent
            }
            Scene = scene;
            Scene.StartPlay();
            Levels.WorldNavigator.AfterSceneLoaded(Scene);   // grupo no SpawnPoint
            SnapCamera();
            SceneLoaded?.Invoke(Scene);
        }

        /// <summary>Atualiza a cena, a câmera e trata transições. Chame após <c>Input.Update()</c>.</summary>
        public void Update(GameTime gameTime)
        {
            var size = LogicalSize();
            Screen.Set(size.X, size.Y);

            Diagnostics.Profiler.Begin("update");
            Scene.Update(gameTime);
            Diagnostics.Profiler.End("update");

            if (Scene.PendingSceneLoad is { } next)
            {
                var path = SceneFileResolver?.Invoke(next)
                           ?? (_sceneFolder != null ? Path.Combine(_sceneFolder, next) : Assets.AssetPaths.Resolve(next));
                if (path != null && File.Exists(path))
                {
                    LoadScene(path);
                    Levels.WorldNavigator.CurrentSceneFile = next;
                }
                else
                    Scene.ClearPendingSceneLoad();
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            DriveCamera(dt, size);
            Screen.CameraPosition = Camera.Position;
            Screen.CameraZoom = Camera.Zoom;

            UpdateDebugOverlay(dt);
        }

        /// <summary>Desenha a cena no backbuffer (com resolução virtual e pós-processo, se houver).</summary>
        public void Draw()
        {
            if (_device == null || _batch == null)
                throw new InvalidOperationException("SceneHost.Initialize não foi chamado.");

            Diagnostics.Profiler.Begin("draw");
            var pp = _device.PresentationParameters;
            int bw = pp.BackBufferWidth, bh = pp.BackBufferHeight;
            var size = LogicalSize();
            var post = _postEffect != null ? FindFirst<PostProcess>(Scene.Objects, p => p.Enabled) : null;
            bool offscreen = post != null || size.X != bw || size.Y != bh;

            if (!offscreen)
            {
                PresentRect = new Rectangle(0, 0, bw, bh);
                _renderer.Render(Scene, Camera, bw, bh);
                Diagnostics.Profiler.End("draw");
                return;
            }

            if (_target == null || _target.Width != size.X || _target.Height != size.Y)
            {
                _target?.Dispose();
                _target = new RenderTarget2D(_device, size.X, size.Y, false, SurfaceFormat.Color, DepthFormat.None);
            }

            _device.SetRenderTarget(_target);
            _renderer.Render(Scene, Camera, size.X, size.Y);
            _device.SetRenderTarget(null);
            _device.Clear(LetterboxColor);

            PresentRect = Letterbox(size, bw, bh);
            if (post != null)
            {
                var projection = Matrix.CreateOrthographicOffCenter(0, bw, bh, 0, 0, 1);
                _postEffect!.Parameters["MatrixTransform"]?.SetValue(projection);
                _postEffect.Parameters["Saturation"]?.SetValue(post.Saturation);
                _postEffect.Parameters["Tint"]?.SetValue(new Vector3(post.Tint.R / 255f, post.Tint.G / 255f, post.Tint.B / 255f));
                _batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, UpscaleSampler, null, null, _postEffect);
            }
            else
            {
                _batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, UpscaleSampler);
            }
            _batch.Draw(_target, PresentRect, Color.White);
            _batch.End();
            Diagnostics.Profiler.End("draw");
        }

        /// <summary>Converte um ponto da janela para coordenadas lógicas (resolução virtual).</summary>
        public Vector2 WindowToLogical(Vector2 window)
        {
            var size = LogicalSize();
            var r = PresentRect;
            if (r.Width <= 0 || r.Height <= 0)
                return window;
            return new Vector2((window.X - r.X) * size.X / r.Width, (window.Y - r.Y) * size.Y / r.Height);
        }

        /// <summary>Converte um ponto lógico (tela) em coordenadas de mundo pela câmera atual.</summary>
        public Vector2 LogicalToWorld(Vector2 logical)
        {
            var size = LogicalSize();
            return Camera.ScreenToWorld(logical, size.X, size.Y);
        }

        /// <summary>Tamanho lógico em que a cena é desenhada.</summary>
        public Point LogicalSize()
        {
            if (VirtualResolution is { } v && v.X > 0 && v.Y > 0)
                return v;
            if (_device == null)
                return new Point(Screen.Width, Screen.Height);
            var pp = _device.PresentationParameters;
            return new Point(Math.Max(1, pp.BackBufferWidth), Math.Max(1, pp.BackBufferHeight));
        }

        /// <summary>Retângulo centralizado que preserva a proporção de <paramref name="size"/>.</summary>
        public static Rectangle Letterbox(Point size, int windowWidth, int windowHeight)
        {
            float scale = Math.Min(windowWidth / (float)size.X, windowHeight / (float)size.Y);
            int w = (int)Math.Round(size.X * scale), h = (int)Math.Round(size.Y * scale);
            return new Rectangle((windowWidth - w) / 2, (windowHeight - h) / 2, w, h);
        }

        /// <summary>Posiciona a câmera imediatamente no alvo (sem suavização), ex.: ao trocar de cena.</summary>
        public void SnapCamera()
        {
            var size = LogicalSize();
            var cam = ActiveCamera();
            if (cam != null)
                cam.SnapCamera(Camera, size.X, size.Y);
            else if (FindFirst<PlatformerController>(Scene.Objects, _ => true) is { } p)
                Camera.Position = p.Owner.Transform.WorldPosition;
        }

        private void DriveCamera(float dt, Point size)
        {
            var cam = ActiveCamera();
            if (cam != null)
                cam.DriveCamera(Camera, dt, size.X, size.Y);
            else if (FindFirst<PlatformerController>(Scene.Objects, _ => true) is { } p)
                Camera.Position = p.Owner.Transform.WorldPosition;
        }

        /// <summary>Câmera ativa (Enabled) de maior prioridade.</summary>
        public CameraComponent? ActiveCamera()
        {
            CameraComponent? best = null;
            foreach (var cam in FindAll<CameraComponent>(Scene.Objects))
                if (cam.Enabled && (best == null || cam.Priority > best.Priority))
                    best = cam;
            return best;
        }

        private bool _f3Prev;

        private void UpdateDebugOverlay(float dt)
        {
            if (dt > 0f)
                _fps = _fps <= 0f ? 1f / dt : _fps * 0.9f + (1f / dt) * 0.1f;

            if (DebugOverlayHotkey)
            {
                bool f3 = Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F3);
                if (f3 && !_f3Prev)
                    _renderer.ShowDebugOverlay = !_renderer.ShowDebugOverlay;
                _f3Prev = f3;
            }

            int count = 0;
            foreach (var _ in EnumerateAll(Scene.Objects)) count++;
            Diagnostics.Profiler.SetCounter("obj", count);
            Diagnostics.Profiler.SetCounter("fps", _fps);
            Diagnostics.Profiler.EndFrame();

            if (_renderer.ShowDebugOverlay)
            {
                var lines = new List<string> { $"CENA {Scene.Name}" };
                lines.AddRange(Diagnostics.Profiler.Report());
                _renderer.DebugLines = lines;
            }
        }

        /// <summary>Todos os objetos (incluindo filhos), em profundidade.</summary>
        public static IEnumerable<GameObject> EnumerateAll(IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var child in EnumerateAll(obj.Children))
                    yield return child;
            }
        }

        /// <summary>Todos os componentes do tipo <typeparamref name="T"/> na árvore.</summary>
        public static IEnumerable<T> FindAll<T>(IEnumerable<GameObject> objects) where T : SceneComponent
        {
            foreach (var obj in EnumerateAll(objects))
                foreach (var c in obj.Components)
                    if (c is T t)
                        yield return t;
        }

        private static T? FindFirst<T>(IEnumerable<GameObject> objects, Func<T, bool> predicate) where T : SceneComponent
        {
            foreach (var c in FindAll<T>(objects))
                if (predicate(c))
                    return c;
            return null;
        }

        public void Dispose()
        {
            _target?.Dispose();
            _batch?.Dispose();
            _renderer.Dispose();
        }
    }
}
