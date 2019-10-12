using System;
using System.Collections.Generic;
using DreamBit.Game.Elements.Components;
using Newtonsoft.Json;

namespace DreamBit.Game.Elements
{
    public interface IGameObject
    {
        Guid Id { get; }
        string Name { get; set; }
        Transform Transform { get; }
        bool IsVisible { get; set; }
        IGameObject Parent { get; set; }
        IReadOnlyList<IGameObject> Children { get; }
        IReadOnlyList<GameObjectComponent> Components { get; }

        void AddChild(IGameObject gameObject);
        void RemoveChild(IGameObject gameObject);

        void AddComponent(GameObjectComponent component);
        void RemoveComponent(GameObjectComponent component);
    }

    public sealed partial class GameObject : IGameObject
    {
        private readonly List<IGameObject> _children;
        private readonly List<GameObjectComponent> _components;
        private IGameObject _parent;

        public GameObject()
        {
            _children = new List<IGameObject>();
            _components = new List<GameObjectComponent>();

            Id = Guid.NewGuid();
            Transform = new Transform();
            IsVisible = true;
        }

        [JsonProperty]
        public Guid Id { get; internal set; }
        [JsonProperty]
        public string Name { get; set; }
        public Transform Transform { get; }
        public bool IsVisible { get; set; }
        public IGameObject Parent
        {
            get => _parent;
            set
            {
                if (value == _parent)
                    return;

                _parent?.RemoveChild(this);
                _parent = value;
                _parent?.AddChild(this);

                Transform.BaseTransform = value?.Transform;
            }
        }
        [JsonIgnore]
        public IReadOnlyList<IGameObject> Children => _children;
        [JsonIgnore]
        public IReadOnlyList<GameObjectComponent> Components => _components;

        public void AddChild(IGameObject gameObject)
        {
            if (!_children.Contains(gameObject))
            {
                _children.Add(gameObject);
                gameObject.Parent = this;
            }
        }
        public void RemoveChild(IGameObject gameObject)
        {
            if (_children.Remove(gameObject))
                gameObject.Parent = null;
        }

        public void AddComponent(GameObjectComponent component)
        {
            if (component.GameObject == this || component.GameObject == null)
            {
                if (!_components.Contains(component))
                    _components.Add(component);
            }
        }
        public void RemoveComponent(GameObjectComponent component)
        {
            if (component.GameObject == this || component.GameObject == null)
                _components.Remove(component);
        }
    }
}