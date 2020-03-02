using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;

namespace DreamBit.Game.Helpers
{
    public static class GameObjectHelper
    {
        public static T GetComponent<T>(this IGameObject gameObject)
        {
            return gameObject.Components.OfType<T>().FirstOrDefault();
        }
        public static T AddComponent<T>(this IGameObject gameObject) where T : GameObjectComponent, new()
        {
            var component = new T();
            gameObject.AddComponent(component);

            return component;
        }
        public static T RemoveComponent<T>(this IGameObject gameObject) where T : GameObjectComponent
        {
            var component = gameObject.GetComponent<T>();

            if (component != null)
                gameObject.RemoveComponent(component);

            return component;
        }
        public static T FindComponent<T>(this IGameObject gameObject)
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
                return component;

            foreach (var child in gameObject.Children)
            {
                var childComponent = child.FindComponent<T>();
                if (childComponent != null)
                    return childComponent;
            }

            return default(T);
        }

        public static IGameObject FindChild(this IGameObject gameObject, string name)
        {
            return gameObject.Children.Find(name);
        }
        public static IGameObject Find(this IEnumerable<IGameObject> gameObjects, string name)
        {
            return gameObjects.Find(g => g.Name == name);
        }
        public static IGameObject Find(this IEnumerable<IGameObject> gameObjects, Guid id)
        {
            return gameObjects.Find(g => g.Id == id);
        }
        private static IGameObject Find(this IEnumerable<IGameObject> gameObjects, Func<IGameObject, bool> equality)
        {
            foreach (var gameObject in gameObjects)
            {
                var found = equality(gameObject) ? gameObject : gameObject.Children.Find(equality);
                if (found != null)
                    return found;
            }

            return null;
        }

        public static void Initialize(this IGameObject gameObject)
        {
            for (var i = 0; i < gameObject.Components.Count; i++)
                gameObject.Components[i].Initialize(gameObject);

            gameObject.Children.InitializeAll();
        }
        public static void InitializeAll(this IReadOnlyList<IGameObject> gameObjects)
        {
            for (var i = 0; i < gameObjects.Count; i++)
                gameObjects[i].Initialize();
        }
    }
}