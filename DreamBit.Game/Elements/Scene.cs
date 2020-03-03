namespace DreamBit.Game.Elements
{
    public sealed class Scene
    {
        public Scene()
        {
            Objects = new GameObjectCollection();
        }

        public IGameObjectCollection Objects { get; }
    }
}
