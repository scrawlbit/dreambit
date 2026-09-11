using System.IO;
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

        public PlayerGame(string? scenePath)
        {
            _scenePath = scenePath;
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            IsMouseVisible = true;
            Window.Title = "DreamBit Player";
        }

        protected override void LoadContent()
        {
            _renderer.Initialize(GraphicsDevice);
            _renderer.ShowGrid = false;

            _scene = _scenePath != null && File.Exists(_scenePath)
                ? SceneSerializer.Load(_scenePath)
                : BuildFallbackScene();

            _scene.StartPlay(); // dispara sons iniciais, reseta estados
        }

        protected override void Update(GameTime gameTime)
        {
            _scene.Update(gameTime);

            // Câmera segue o personagem (primeiro objeto com PlatformerController).
            var target = FindPlayer(_scene.Objects);
            if (target != null)
                _camera.Position = target.Transform.WorldPosition;

            base.Update(gameTime);
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
