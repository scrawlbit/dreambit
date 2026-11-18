using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Hosting
{
    /// <summary>
    /// Classe base para um jogo MonoGame próprio que usa a DreamBit: herde, configure em
    /// <see cref="Configure"/> e carregue a primeira cena em <see cref="OnStart"/>.
    /// A DreamBit cuida de cena/mapas/animação/física/câmera/áudio; a lógica do jogo fica em
    /// componentes C# do próprio jogo (ver <c>ComponentRegistry</c>) e nos ganchos abaixo.
    /// </summary>
    /// <example>
    /// <code>
    /// public sealed class MeuJogo : DreamBitGame
    /// {
    ///     protected override void Configure(GameSettings s) { s.Title = "Meu jogo"; s.VirtualResolution = new Point(1920, 1080); }
    ///     protected override void OnStart() => Host.LoadScene("Cenas/fase1.dbscene");
    /// }
    /// </code>
    /// </example>
    public abstract class DreamBitGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly GameSettings _settings = new();

        protected DreamBitGame()
        {
            Configure(_settings);
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = _settings.WindowWidth,
                PreferredBackBufferHeight = _settings.WindowHeight,
                IsFullScreen = _settings.FullScreen,
                SynchronizeWithVerticalRetrace = _settings.VSync,
                HardwareModeSwitch = false
            };
            IsMouseVisible = _settings.MouseVisible;
            IsFixedTimeStep = _settings.FixedTimeStep;
            Window.Title = _settings.Title;
            Window.AllowUserResizing = _settings.AllowResizing;
            Content.RootDirectory = _settings.ContentDirectory;
            foreach (var root in _settings.AssetRoots)
                Assets.AssetPaths.AddRoot(root);
            Window.TextInput += (_, e) => Input.Input.PushText(e.Character);
        }

        /// <summary>Runtime da cena (carregar cenas, câmera, desenho).</summary>
        public SceneHost Host { get; } = new();

        /// <summary>Cena em execução.</summary>
        public Scene Scene => Host.Scene;

        /// <summary>Gerenciador gráfico (resolução, tela cheia).</summary>
        protected GraphicsDeviceManager Graphics => _graphics;

        /// <summary>Configurações iniciais (janela, resolução virtual, raízes de assets).</summary>
        protected virtual void Configure(GameSettings settings) { }

        /// <summary>Chamado uma vez com o dispositivo gráfico pronto: carregue a primeira cena.</summary>
        protected abstract void OnStart();

        /// <summary>Antes da atualização da cena (input já lido neste frame).</summary>
        protected virtual void OnPreUpdate(GameTime gameTime) { }

        /// <summary>Depois da atualização da cena e da câmera.</summary>
        protected virtual void OnPostUpdate(GameTime gameTime) { }

        /// <summary>Depois de desenhar a cena (desenho extra em espaço de janela).</summary>
        protected virtual void OnPostDraw(GameTime gameTime) { }

        /// <summary>Alterna tela cheia (sem trocar o modo de vídeo).</summary>
        public void ToggleFullScreen()
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            if (!_graphics.IsFullScreen)
            {
                _graphics.PreferredBackBufferWidth = _settings.WindowWidth;
                _graphics.PreferredBackBufferHeight = _settings.WindowHeight;
            }
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            Host.VirtualResolution = _settings.VirtualResolution;
            Host.UpscaleSampler = _settings.SmoothUpscale
                ? Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp
                : Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp;
            Host.Renderer.Background = _settings.ClearColor;
            Rendering.DebugDraw.Colliders = _settings.ShowDebugShapes;
            Rendering.DebugDraw.Zones = _settings.ShowDebugShapes;
            Saving.SaveGame.GameFolderName = _settings.SaveFolderName ?? _settings.Title;
            Host.Initialize(GraphicsDevice, Content);
            OnStart();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Input.Update();
            Input.Input.PointerMapper = Host.WindowToLogical;
            OnPreUpdate(gameTime);
            Host.Update(gameTime);
            OnPostUpdate(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Host.Draw();
            OnPostDraw(gameTime);
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            Host.Dispose();
            base.UnloadContent();
        }
    }

    /// <summary>Configurações iniciais de um <see cref="DreamBitGame"/>.</summary>
    public sealed class GameSettings
    {
        public string Title { get; set; } = "DreamBit";
        public int WindowWidth { get; set; } = 1280;
        public int WindowHeight { get; set; } = 720;
        public bool FullScreen { get; set; }
        public bool VSync { get; set; } = true;
        public bool FixedTimeStep { get; set; } = true;
        public bool AllowResizing { get; set; } = true;
        public bool MouseVisible { get; set; } = true;

        /// <summary>Resolução lógica (desenho com letterbox). Null = tamanho da janela.</summary>
        public Point? VirtualResolution { get; set; }

        /// <summary>Escala suave (arte pintada) ou nearest (pixel art) ao aplicar a resolução virtual.</summary>
        public bool SmoothUpscale { get; set; } = true;

        public Color ClearColor { get; set; } = Color.Black;

        /// <summary>Desenha colisores e zonas (protótipo). Desligado num jogo final.</summary>
        public bool ShowDebugShapes { get; set; }

        /// <summary>Pasta dos saves em %AppData% (padrão: o título).</summary>
        public string? SaveFolderName { get; set; }

        /// <summary>Pasta do ContentManager do MonoGame (shaders .xnb, opcional).</summary>
        public string ContentDirectory { get; set; } = "Content";

        /// <summary>Raízes para resolver caminhos relativos de assets das cenas.</summary>
        public System.Collections.Generic.List<string> AssetRoots { get; } = new();
    }
}
