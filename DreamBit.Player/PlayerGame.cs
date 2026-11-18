using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Hosting;
using Microsoft.Xna.Framework;

namespace DreamBit.Player
{
    /// <summary>
    /// Jogo MonoGame que carrega uma cena .dbscene e a executa (Update + Draw).
    /// É o "play" fora do editor — o mesmo motor (DreamBit.Engine), sobre o
    /// <see cref="DreamBitGame"/> que qualquer jogo pode usar.
    /// </summary>
    public sealed class PlayerGame : DreamBitGame
    {
        private readonly string? _scenePath;

        // Modo captura (--shot): renderiza N frames, salva o backbuffer em PNG e sai.
        private readonly string? _shotPath;
        private readonly int _shotFrame;
        private readonly bool _autoWalk;
        private readonly string? _clip;
        private readonly bool _grayscale;
        private int _frames;

        // Exemplo da galeria a rodar (ex.: --example 09-audio), ou null.
        private readonly string? _example;

        public PlayerGame(string? scenePath, string? shotPath = null, int shotFrame = 110, bool autoWalk = false, string? clip = null, bool grayscale = false, string? example = null)
        {
            _grayscale = grayscale;
            _example = example;
            _scenePath = scenePath;
            _shotPath = shotPath;
            _shotFrame = shotFrame;
            _autoWalk = autoWalk;
            _clip = clip;
            if (scenePath != null && Path.GetDirectoryName(Path.GetFullPath(scenePath)) is { } folder)
                Engine.Assets.AssetPaths.AddRoot(folder);
        }

        protected override void Configure(GameSettings settings)
        {
            settings.Title = "DreamBit Player";
            settings.ClearColor = new Color(24, 26, 32);
            settings.AllowResizing = false;
        }

        protected override void OnStart()
        {
            Host.Renderer.ShowLedges = true; // o Player sempre mostrou as ledges (protótipos sem arte)
            Host.SceneLoaded += scene => Window.Title = "DreamBit Player — " + scene.Name;

            if (_example != null)
                Host.SetScene(ExampleScenes.Get(_example) ?? BuildFallbackScene());
            else if (_scenePath != null && File.Exists(_scenePath))
                Host.LoadScene(Path.GetFullPath(_scenePath));
            else
                Host.SetScene(BuildFallbackScene());

            if (_grayscale)
            {
                var fx = new GameObject("PostFX");
                fx.AddComponent(new PostProcess { Saturation = 0f });
                Scene.Add(fx);
            }

            // Modo captura: força um clipe de skeleton (para fotografar idle/walk/attack/jump).
            if (_clip != null)
                foreach (var sk in SceneHost.FindAll<SkeletonAnimator>(Scene.Objects))
                    sk.Play(_clip);
        }

        protected override void OnPreUpdate(GameTime gameTime)
        {
            _frames++;
            // Captura: deixa assentar ~40 frames e então "anda" para a direita (clipe walk).
            if (_shotPath != null && _autoWalk && _frames > 40)
                Engine.Input.Input.HoldAction("MoveRight");
        }

        protected override void OnPostDraw(GameTime gameTime)
        {
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
