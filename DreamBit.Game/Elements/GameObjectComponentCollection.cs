using Scrawlbit.Collections;
using Scrawlbit.Helpers;
using System.Collections.Generic;

namespace DreamBit.Game.Elements
{
    public interface IGameObjectComponentCollection : IReadOnlyObservableCollection<GameObjectComponent>
    {
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
