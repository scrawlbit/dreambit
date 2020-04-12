namespace DreamBit.Game.Elements
{
    public sealed class Scene
    {
        public Scene()
        {
            Objects = new GameObjectCollection();
        }

        public IGameObjectCollection Objects { get; }

        public void Preview()
        {
            for (int i = 0; i < Objects.Count; i++)
                Objects[i].Preview();
        }
    }
}
