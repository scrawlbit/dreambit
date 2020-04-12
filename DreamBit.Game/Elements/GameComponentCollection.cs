using Scrawlbit.Collections;
using System.Linq;

namespace DreamBit.Game.Elements
{
    public interface IGameComponentCollection : IReadOnlyObservableCollection<GameComponent>
    {
        bool Contains<TComponent>() where TComponent : GameComponent;

        void Add(params GameComponent[] components);
        void Remove(GameComponent component);
    }

    internal class GameComponentCollection : ExtendedObservableCollection<GameComponent>, IGameComponentCollection
    {
        private readonly GameObject _target;

        public GameComponentCollection(GameObject target)
        {
            _target = target;
        }

        public bool Contains<TComponent>() where TComponent : GameComponent
        {
            return this.Any(c => c is TComponent);
        }

        void IGameComponentCollection.Add(params GameComponent[] components)
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
        void IGameComponentCollection.Remove(GameComponent component)
        {
            if (component.GameObject == _target || component.GameObject == null)
                Remove(component);
        }
    }
}
