namespace DreamBit.Game.Elements.Components
{
    public abstract partial class GameObjectComponent
    {
        public bool Started { get; private set; }
        public IGameObject GameObject { get; private set; }

        internal virtual void Initialize(IGameObject gameObject)
        {
            if (!Started)
            {
                GameObject = gameObject;

                Start();
                Started = true;
            }
        }

        protected internal virtual void Start() { }
        protected internal virtual void Update() { }
        protected internal virtual void PostUpdate() { }
        protected internal virtual void Draw() { }
    }
}