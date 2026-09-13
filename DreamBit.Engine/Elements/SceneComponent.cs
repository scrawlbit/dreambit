using DreamBit.Engine.Notification;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Elements
{
    /// <summary>
    /// Base de componente de um GameObject. Equivalente ao SceneComponent de DreamBit.Game:
    /// participa do laço de update e do desenho da cena.
    /// </summary>
    public abstract class SceneComponent : NotificationObject
    {
        private bool _enabled = true;

        /// <summary>Objeto dono deste componente (atribuído pela coleção).</summary>
        public GameObject Owner { get; internal set; } = null!;

        /// <summary>Nome exibido no inspector.</summary>
        public abstract string DisplayName { get; }

        /// <summary>Se o componente participa do laço (Update/Draw). Desligue para pausar um
        /// comportamento sem removê-lo — ex.: numa fase de voo, desligar o
        /// <c>PlatformerController</c> (gravidade). Pode ser alternado em runtime.</summary>
        public bool Enabled { get => _enabled; set => Set(ref _enabled, value); }

        /// <summary>Chamado quando o play mode inicia (resetar estado, tocar som inicial, etc.).</summary>
        protected internal virtual void OnPlayStarted() { }

        protected internal virtual void Update(GameTime gameTime) { }

        protected internal virtual void Draw(ISceneDrawing drawing) { }

        /// <summary>Desenho em espaço de tela (HUD), fora da transformação de câmera.</summary>
        protected internal virtual void DrawScreen(ISceneDrawing drawing) { }
    }
}
