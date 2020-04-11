using Scrawlbit.Collections;
using System.Linq;

namespace DreamBit.Game.Elements
{
    public interface IGameObjectComponentCollection : IReadOnlyObservableCollection<GameObjectComponent>
    {
        bool Contains<TComponent>() where TComponent : GameObjectComponent;

        void Add(params GameObjectComponent[] components);
        void Remove(GameObjectComponent component);
    }

    internal class GameObjectComponentCollection : ExtendedObservableCollection<GameObjectComponent>, IGameObjectComponentCollection
    {
        private readonly GameObject _target;

        public GameObjectComponentCollection(GameObject target)
        {
            _target = target;
        }

        public bool Contains<TComponent>() where TComponent : GameObjectComponent
        {
            return this.Any(c => c is TComponent);
        }

        void IGameObjectComponentCollection.Add(params GameObjectComponent[] components)
        {
            foreach (var component in components)
            {
                if (component.GameObject == _target || component.GameObject == null)
                {
                    if (!Contains(component))
                        Add(component);
                }
            }
        }
        void IGameObjectComponentCollection.Remove(GameObjectComponent component)
        {
            if (component.GameObject == _target || component.GameObject == null)
                Remove(component);
        }
    }
}
