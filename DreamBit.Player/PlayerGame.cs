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
        private int _frames;

        public PlayerGame(string? scenePath, string? shotPath = null, int shotFrame = 110, bool autoWalk = false, string? clip = null)
        {
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

        protected override void LoadContent()
        {
            _renderer.Initialize(GraphicsDevice);
            _renderer.ShowGrid = false;

            _sceneFolder = _scenePath != null ? Path.GetDirectoryName(_scenePath) : null;

            _scene = _scenePath != null && File.Exists(_scenePath)
                ? SceneSerializer.Load(_scenePath)
                : BuildFallbackScene();

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

            _scene.Update(gameTime);

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

            UpdateDebugOverlay(gameTime);

            base.Update(gameTime);
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

            if (_renderer.ShowDebugOverlay)
                _renderer.DebugLines = new[]
                {
                    $"FPS {_fps:0}",
                    $"OBJ {CountObjects(_scene.Objects)}",
                    $"CENA {_scene.Name}"
                };
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

        protected override void Draw(GameTime gameTime)
        {
            _renderer.Render(_scene, _camera,
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
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
