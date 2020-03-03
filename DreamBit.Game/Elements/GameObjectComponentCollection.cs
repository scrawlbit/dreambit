using Scrawlbit.Collections;

namespace DreamBit.Game.Elements
{
    public interface IGameObjectComponentCollection : IReadOnlyObservableCollection<GameObjectComponent>
    {
        void Add(GameObjectComponent component);
        void Remove(GameObjectComponent component);
    }

    internal class GameObjectComponentCollection : ExtendedObservableCollection<GameObjectComponent>, IGameObjectComponentCollection
    {
        private readonly GameObject _target;

        public GameObjectComponentCollection(GameObject target)
        {
            _target = target;
        }

        void IGameObjectComponentCollection.Add(GameObjectComponent component)
        {
            if (component.GameObject == _target || component.GameObject == null)
            {
                if (!Contains(component))
                    Add(component);
            }
        }
        void IGameObjectComponentCollection.Remove(GameObjectComponent component)
        {
            if (component.GameObject == _target || component.GameObject == null)
                Remove(component);
        }
    }
}
