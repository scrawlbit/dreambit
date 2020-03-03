﻿using Scrawlbit.Collections;

namespace DreamBit.Game.Elements
{
    public interface IGameObjectCollection : IReadOnlyObservableCollection<GameObject>
    {
        void Add(GameObject gameObject);
        void Remove(GameObject gameObject);
    }

    internal class GameObjectCollection : ExtendedObservableCollection<GameObject>, IGameObjectCollection
    {
        private readonly GameObject _parent;

        public GameObjectCollection(GameObject parent = null)
        {
            _parent = parent;
        }

        void IGameObjectCollection.Add(GameObject gameObject)
        {
            if (!Contains(gameObject))
            {
                Add(gameObject);
                gameObject.Parent = _parent;
            }
        }
        void IGameObjectCollection.Remove(GameObject gameObject)
        {
            if (Remove(gameObject))
                gameObject.Parent = null;
        }
    }
}
