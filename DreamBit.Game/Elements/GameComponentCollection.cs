using Scrawlbit.Collections;
using System.Linq;

namespace DreamBit.Game.Elements
{
    public interface IGameComponentCollection : IReadOnlyObservableCollection<GameComponent>
    {
        T Find<T>() where T : GameComponent;
        bool Contains<T>() where T : GameComponent;

        void Add(params GameComponent[] components);
        void Remove(GameComponent component);
        void Insert(int index, GameComponent component);
    }

    internal class GameComponentCollection : ExtendedObservableCollection<GameComponent>, IGameComponentCollection
    {
        private readonly GameObject _target;

        public GameComponentCollection(GameObject target)
        {
            _target = target;
        }

        public T Find<T>() where T : GameComponent
        {
            return this.OfType<T>().FirstOrDefault();
        }
        public bool Contains<TComponent>() where TComponent : GameComponent
        {
            return this.Any(c => c is TComponent);
        }

        void IGameComponentCollection.Add(params GameComponent[] components)
        {
            foreach (var component in components)
            {
                if (!Contains(component) && (component.GameObject == _target || component.GameObject == null))
                {
                    Add(component);
                    component.GameObject = _target;
                }
            }
        }
        void IGameComponentCollection.Remove(GameComponent component)
        {
            if (component.GameObject == _target || component.GameObject == null)
                Remove(component);
        }
        void IGameComponentCollection.Insert(int index, GameComponent component)
        {
            if (!Contains(component))
            {
                Insert(index, component);
                component.GameObject = _target;
            }
        }
    }
}
