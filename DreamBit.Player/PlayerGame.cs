using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;

namespace DreamBit.Player
{
    /// <summary>
    /// Jogo MonoGame que carrega uma cena .dbscene e a executa (Update + Draw).
    /// É o "play" fora do editor — o mesmo motor (DreamBit.Engine), sem WPF.
    /// </summary>
    public sealed class PlayerGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly string? _scenePath;
        private readonly SceneRenderer _renderer = new();
        private Scene _scene = new();
        private readonly Camera2D _camera = new();

        // Modo captura (--shot): renderiza N frames, salva o backbuffer em PNG e sai.
        private readonly string? _shotPath;
        private readonly int _shotFrame;
        private readonly bool _autoWalk;
        private readonly string? _clip;
        private readonly bool _grayscale;
        private int _frames;

        // Demo por código (ex.: --music-demo) e log de intensidade da música (--music-log).
        private readonly string? _demo;
        private readonly string? _example;
        private readonly float _musicLog;
        private float _musicLogElapsed;
        private float _musicLogNextPrint;

        public PlayerGame(string? scenePath, string? shotPath = null, int shotFrame = 110, bool autoWalk = false, string? clip = null, bool grayscale = false, string? demo = null, float musicLog = 0f, string? example = null)
        {
            _grayscale = grayscale;
            _demo = demo;
            _example = example;
            _musicLog = musicLog;
            _scenePath = scenePath;
            _shotPath = shotPath;
            _shotFrame = shotFrame;
            _autoWalk = autoWalk;
            _clip = clip;
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            IsMouseVisible = true;
            Window.Title = "DreamBit Player";
            Window.TextInput += (_, e) => DreamBit.Engine.Input.Input.PushText(e.Character); // campos de texto de UI
        }

        private string? _sceneFolder;

        private Microsoft.Xna.Framework.Content.ContentManager? _content;
        private Microsoft.Xna.Framework.Graphics.Effect? _postEffect;
        private Microsoft.Xna.Framework.Graphics.SpriteBatch? _postBatch;
        private Microsoft.Xna.Framework.Graphics.RenderTarget2D? _sceneTarget;

        protected override void LoadContent()
        {
            _renderer.Initialize(GraphicsDevice);
            _renderer.ShowGrid = false;

            // Shader de pós-processamento (opcional): carrega o efeito compilado (MGCB), se houver.
            try
            {
                _content = new Microsoft.Xna.Framework.Content.ContentManager(Services, "Content");
                _postEffect = _content.Load<Microsoft.Xna.Framework.Graphics.Effect>("PostProcess");
                _postBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);
            }
            catch { _postEffect = null; /* sem shader compilado: renderiza normal */ }

            _sceneFolder = _scenePath != null ? Path.GetDirectoryName(_scenePath) : null;

            _scene = _example != null
                ? ExampleScenes.Get(_example) ?? BuildFallbackScene()
                : _demo == "music"
                    ? DemoScenes.AdaptiveMusic()
                    : _scenePath != null && File.Exists(_scenePath)
                        ? SceneSerializer.Load(_scenePath)
                        : BuildFallbackScene();

            if (_grayscale)
            {
                var fx = new GameObject("PostFX");
                fx.AddComponent(new DreamBit.Engine.Components.PostProcess { Saturation = 0f });
                _scene.Add(fx);
            }

            _scene.StartPlay(); // dispara sons iniciais, reseta estados

            // Modo captura: força um clipe de skeleton (para fotografar idle/walk/attack/jump).
            if (_clip != null)
                foreach (var obj in EnumerateAll(_scene.Objects))
                    foreach (var sk in obj.Components.OfType<DreamBit.Engine.Components.SkeletonAnimator>())
                        sk.Play(_clip);
        }

        private static System.Collections.Generic.IEnumerable<GameObject> EnumerateAll(System.Collections.Generic.IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                yield return obj;
                foreach (var c in EnumerateAll(obj.Children))
                    yield return c;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            DreamBit.Engine.Rendering.Screen.Set(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            DreamBit.Engine.Input.Input.Update(); // snapshot de teclado/gamepad/mouse do frame

            _frames++;
            // Captura: deixa assentar ~40 frames e então "anda" para a direita (clipe walk).
            if (_shotPath != null && _autoWalk && _frames > 40)
                DreamBit.Engine.Input.Input.HoldAction("MoveRight");

            DreamBit.Engine.Diagnostics.Profiler.Begin("update");
            _scene.Update(gameTime);
            DreamBit.Engine.Diagnostics.Profiler.End("update");

            // Transição de fase: um SceneExit pediu para carregar outra cena.
            if (_scene.PendingSceneLoad is { } next)
                LoadNextScene(next);

            // Câmera: usa um CameraComponent da cena, se houver; senão segue o Platformer.
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var camera = FindCamera(_scene.Objects);
            if (camera != null)
                camera.DriveCamera(_camera, dt, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            else
            {
                var target = FindPlayer(_scene.Objects);
                if (target != null)
                    _camera.Position = target.Transform.WorldPosition;
            }

            DreamBit.Engine.Rendering.Screen.CameraPosition = _camera.Position; // parallax do próximo frame
            DreamBit.Engine.Rendering.Screen.CameraZoom = _camera.Zoom;         // visão de câmera (o que está na tela)

            UpdateDebugOverlay(gameTime);

            if (_musicLog > 0f)
                LogMusic(dt);

            base.Update(gameTime);
        }

        // ---- log de intensidade da música adaptativa (--music-log) ----
        private void LogMusic(float dt)
        {
            _musicLogElapsed += dt;
            if (_musicLogElapsed >= _musicLogNextPrint)
            {
                _musicLogNextPrint += 0.25f;
                var music = EnumerateAll(_scene.Objects)
                    .SelectMany(o => o.Components)
                    .OfType<DreamBit.Engine.Components.LayeredMusic>()
                    .FirstOrDefault();
                int onScreen = DreamBit.Engine.Elements.CameraVision.CountOnScreen(_scene, "Inimigo");
                float drums = music != null && music.Layers.Count > 0 ? music.Layers[0].Current : 0f;
                float baseVol = music?.BaseVolume ?? 0f;
                System.Console.WriteLine(
                    $"t={_musicLogElapsed,5:0.00}s  inimigos_na_tela={onScreen}  base={baseVol:0.00}  bateria={drums:0.00}  {Bar(drums)}");
            }
            if (_musicLogElapsed >= _musicLog)
                Exit();
        }

        private static string Bar(float v)
        {
            int n = (int)System.Math.Round(v * 20f);
            n = System.Math.Max(0, System.Math.Min(20, n));
            return new string('#', n);
        }

        // ---- overlay de debug (F3) ----
        private float _fps;
        private bool _f3Prev;

        private void UpdateDebugOverlay(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt > 0f)
                _fps = _fps <= 0f ? 1f / dt : _fps * 0.9f + (1f / dt) * 0.1f; // suavizado

            bool f3 = Microsoft.Xna.Framework.Input.Keyboard.GetState()
                .IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F3);
            if (f3 && !_f3Prev)
                _renderer.ShowDebugOverlay = !_renderer.ShowDebugOverlay;
            _f3Prev = f3;

            // Contadores do profiler + fecha o frame.
            DreamBit.Engine.Diagnostics.Profiler.SetCounter("obj", CountObjects(_scene.Objects));
            DreamBit.Engine.Diagnostics.Profiler.SetCounter("fps", _fps);
            DreamBit.Engine.Diagnostics.Profiler.EndFrame();

            if (_renderer.ShowDebugOverlay)
            {
                var lines = new System.Collections.Generic.List<string> { $"CENA {_scene.Name}" };
                lines.AddRange(DreamBit.Engine.Diagnostics.Profiler.Report());
                _renderer.DebugLines = lines;
            }
        }

        private static int CountObjects(System.Collections.Generic.IEnumerable<GameObject> objects)
        {
            int n = 0;
            foreach (var obj in objects)
            {
                n++;
                n += CountObjects(obj.Children);
            }
            return n;
        }

        private void LoadNextScene(string sceneFile)
        {
            var path = _sceneFolder != null ? Path.Combine(_sceneFolder, sceneFile) : sceneFile;
            if (!File.Exists(path))
            {
                _scene.ClearPendingSceneLoad();
                return;
            }

            _scene = SceneSerializer.Load(path);
            _scene.StartPlay();
            Window.Title = "DreamBit Player — " + Path.GetFileNameWithoutExtension(path);
        }

        private static CameraComponent? FindCamera(System.Collections.Generic.IEnumerable<GameObject> objects)
        {
            // Câmera ativa (Enabled) de maior prioridade — permite trocar de câmera.
            CameraComponent? best = null;
            void Scan(System.Collections.Generic.IEnumerable<GameObject> objs)
            {
                foreach (var obj in objs)
                {
                    foreach (var component in obj.Components)
                        if (component is CameraComponent cam && cam.Enabled &&
                            (best == null || cam.Priority > best.Priority))
                            best = cam;
                    Scan(obj.Children);
                }
            }
            Scan(objects);
            return best;
        }

        private static GameObject? FindPlayer(System.Collections.Generic.IEnumerable<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                foreach (var component in obj.Components)
                    if (component is PlatformerController)
                        return obj;

                var nested = FindPlayer(obj.Children);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private DreamBit.Engine.Components.PostProcess? FindPostProcess()
        {
            foreach (var obj in EnumerateAll(_scene.Objects))
                foreach (var c in obj.Components)
                    if (c is DreamBit.Engine.Components.PostProcess pp && pp.Enabled)
                        return pp;
            return null;
        }

        protected override void Draw(GameTime gameTime)
        {
            DreamBit.Engine.Diagnostics.Profiler.Begin("draw");
            int w = GraphicsDevice.Viewport.Width, h = GraphicsDevice.Viewport.Height;
            var post = _postEffect != null ? FindPostProcess() : null;

            if (post != null && _postBatch != null)
            {
                // Renderiza a cena num alvo e a desenha com o shader (grayscale/tint).
                if (_sceneTarget == null || _sceneTarget.Width != w || _sceneTarget.Height != h)
                {
                    _sceneTarget?.Dispose();
                    _sceneTarget = new Microsoft.Xna.Framework.Graphics.RenderTarget2D(GraphicsDevice, w, h);
                }
                GraphicsDevice.SetRenderTarget(_sceneTarget);
                _renderer.Render(_scene, _camera, w, h);
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

                // Projeção ortográfica da tela (setar manualmente evita depender do SpriteBatch).
                var projection = Microsoft.Xna.Framework.Matrix.CreateOrthographicOffCenter(0, w, h, 0, 0, 1);
                _postEffect!.Parameters["MatrixTransform"]?.SetValue(projection);
                _postEffect.Parameters["Saturation"]?.SetValue(post.Saturation);
                _postEffect.Parameters["Tint"]?.SetValue(new Microsoft.Xna.Framework.Vector3(
                    post.Tint.R / 255f, post.Tint.G / 255f, post.Tint.B / 255f));
                _postBatch.Begin(Microsoft.Xna.Framework.Graphics.SpriteSortMode.Immediate,
                    Microsoft.Xna.Framework.Graphics.BlendState.Opaque,
                    Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp, null, null, _postEffect);
                _postBatch.Draw(_sceneTarget, new Microsoft.Xna.Framework.Rectangle(0, 0, w, h), Microsoft.Xna.Framework.Color.White);
                _postBatch.End();
            }
            else
            {
                _renderer.Render(_scene, _camera, w, h);
            }
            DreamBit.Engine.Diagnostics.Profiler.End("draw");
            base.Draw(gameTime);

            if (_shotPath != null && _frames >= _shotFrame)
                CaptureAndExit();
        }

        /// <summary>Salva o backbuffer em PNG e encerra (modo --shot).</summary>
        private void CaptureAndExit()
        {
            int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int h = GraphicsDevice.PresentationParameters.BackBufferHeight;
            var data = new Color[w * h];
            GraphicsDevice.GetBackBufferData(data);
            using (var tex = new Microsoft.Xna.Framework.Graphics.Texture2D(GraphicsDevice, w, h))
            {
                tex.SetData(data);
                using var fs = File.Create(_shotPath!);
                tex.SaveAsPng(fs, w, h);
            }
            Exit();
        }

        private static Scene BuildFallbackScene()
        {
            var scene = new Scene { Name = "Sem cena" };
            var obj = new GameObject("Demo");
            obj.AddComponent(new SpriteRenderer { Size = new Vector2(120, 120) });
            obj.AddComponent(new RotatorBehavior { Speed = 1.5f });
            scene.Add(obj);
            return scene;
        }
    }
}
