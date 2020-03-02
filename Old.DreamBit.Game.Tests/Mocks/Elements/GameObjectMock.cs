using System;
using System.Collections.Generic;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;

namespace DreamBit.Game.Tests.Mocks.Elements
{
    public class GameObjectMock : IGameObject
    {
        public GameObjectMock()
        {
            IsVisible = true;
            Children = new List<IGameObject>();
            Components = new List<GameObjectComponent>();
            Transform = new Transform();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Transform Transform { get; set; }
        public bool IsVisible { get; set; }
        public IGameObject Parent { get; set; }
        public List<IGameObject> Children { get; }
        public List<GameObjectComponent> Components { get; }

        void IGameObject.AddChild(IGameObject gameObject)
        {
            throw new NotImplementedException();
        }
        void IGameObject.RemoveChild(IGameObject gameObject)
        {
            throw new NotImplementedException();
        }
        void IGameObject.AddComponent(GameObjectComponent component)
        {
            throw new NotImplementedException();
        }
        void IGameObject.RemoveComponent(GameObjectComponent component)
        {
            throw new NotImplementedException();
        }

        IReadOnlyList<IGameObject> IGameObject.Children => Children;
        IReadOnlyList<GameObjectComponent> IGameObject.Components => Components;
    }
}