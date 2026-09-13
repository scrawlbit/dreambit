using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components
{
    /// <summary>
    /// Ouvinte de áudio 2D: sua posição (a do objeto, normalmente a câmera/herói) é usada pelas
    /// <see cref="AudioSource"/> espaciais para calcular volume e panorâmica. Um por cena.
    /// </summary>
    public sealed class AudioListener : SceneComponent
    {
        /// <summary>Posição do ouvinte no mundo, ou null se nenhum ouvinte ativo neste frame.</summary>
        public static Vector2? Position { get; private set; }

        public override string DisplayName => "Audio Listener";

        protected internal override void OnPlayStarted()
            => Position = Owner?.Transform.WorldPosition;

        protected internal override void Update(GameTime gameTime)
            => Position = Owner?.Transform.WorldPosition;

        /// <summary>Limpa o ouvinte (chamado ao parar o play).</summary>
        public static void Clear() => Position = null;
    }
}
