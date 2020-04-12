namespace DreamBit.Game.Elements
{
    public abstract partial class GameComponent
    {
        public bool Started { get; private set; }
        public GameObject GameObject { get; private set; }

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
        protected internal virtual void Update() { }
        protected internal virtual void PostUpdate() { }
        protected internal virtual void Draw() { }
    }
}