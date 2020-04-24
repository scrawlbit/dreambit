using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;
using Scrawlbit.Notification;

namespace DreamBit.Game.Elements
{
    public abstract partial class GameComponent : NotificationObject
    {
        public GameObject GameObject { get; private set; }
        public bool Started { get; private set; }
        public abstract string Name { get; }

        internal virtual void Initialize(GameObject gameObject)
        {
            if (!Started)
            {
                GameObject = gameObject;

                Start();
                Started = true;
            }
        }

        protected internal virtual void Start() { }
        protected internal virtual void Update(GameTime gameTime) { }
        protected internal virtual void PostUpdate(GameTime gameTime) { }
        protected internal virtual void Draw(IContentDrawer drawer) { }
    }
}