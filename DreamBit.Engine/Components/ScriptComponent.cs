using DreamBit.Engine.Elements;
using DreamBit.Engine.Scripting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Componente de script: compila em runtime (Roslyn) uma classe C# do usuário que
    /// implementa IGameScript e chama Update a cada frame no play. (Marco 3.)
    /// </summary>
    public sealed class ScriptComponent : SceneComponent
    {
        public const string DefaultSource =
            "public class Script : IGameScript\n" +
            "{\n" +
            "    public void Update(GameObject self, float dt)\n" +
            "    {\n" +
            "        // Exemplo: gira o objeto\n" +
            "        self.Transform.Rotation += 1.5f * dt;\n" +
            "    }\n" +
            "}\n";

        private string _source = DefaultSource;
        private IGameScript? _instance;
        private string? _error;
        private bool _compiled;

        public override string DisplayName => "Script";

        public string Source
        {
            get => _source;
            set
            {
                if (Set(ref _source, value ?? string.Empty))
                {
                    _compiled = false;
                    _instance = null;
                }
            }
        }

        /// <summary>Mensagem de erro da última compilação (null se OK).</summary>
        public string? Error
        {
            get => _error;
            private set => Set(ref _error, value);
        }

        protected internal override void OnPlayStarted() => Compile();

        /// <summary>Compila o script (usado ao iniciar o play ou pelo botão "Compilar").</summary>
        public void Compile()
        {
            var (script, error) = ScriptCompiler.Compile(_source);
            _instance = script;
            Error = error;
            _compiled = true;
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (!_compiled)
                Compile();

            _instance?.Update(Owner, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
