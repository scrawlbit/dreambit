using Scrawlbit.Collections;
using System;

namespace DreamBit.Game.Elements
{
    public interface IGameObjectCollection : IReadOnlyObservableCollection<GameObject>
    {
        GameObject Find(Guid id);

        void Add(params GameObject[] gameObjects);
        void Remove(GameObject gameObject);
        void Move(int oldIndex, int newIndex);
        void Insert(int index, GameObject gameObject);
    }

    internal class GameObjectCollection : ExtendedObservableCollection<GameObject>, IGameObjectCollection
    {
        private readonly GameObject _parent;

        public GameObjectCollection(GameObject parent = null)
        {
            _parent = parent;
        }

        GameObject IGameObjectCollection.Find(Guid id)
        {
            foreach (var gameObject in this)
            {
                if (gameObject.Id == id)
                    return gameObject;

                GameObject child = gameObject.Children.Find(id);
                if (child != null)
                    return child;
            }

            return null;
        }

        void IGameObjectCollection.Add(params GameObject[] gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (!Contains(gameObject))
                {
                    Add(gameObject);
                    gameObject.Parent = _parent;
                }
            }
        }
        void IGameObjectCollection.Remove(GameObject gameObject)
        {
            if (Remove(gameObject))
                gameObject.Parent = null;
        }
        void IGameObjectCollection.Insert(int index, GameObject gameObject)
        {
            if (!Contains(gameObject))
            {
                Insert(index, gameObject);
                gameObject.Parent = _parent;
            }
        }
    }
}
