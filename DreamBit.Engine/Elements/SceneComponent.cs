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
        /// <summary>Objeto dono deste componente (atribuído pela coleção).</summary>
        public GameObject Owner { get; internal set; } = null!;

        /// <summary>Nome exibido no inspector.</summary>
        public abstract string DisplayName { get; }

        protected internal virtual void Update(GameTime gameTime) { }

        protected internal virtual void Draw(ISceneDrawing drawing) { }
    }
}
