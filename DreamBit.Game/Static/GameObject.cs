using DreamBit.Game.Elements.Components;
using DreamBit.Game.Helpers;

// ReSharper disable once CheckNamespace
namespace DreamBit.Game.Elements
{
    partial class GameObject
    {
        public static GameObject Copy(IGameObject gameObject)
        {
            var copy = new GameObject
            {
                Name = gameObject.Name,
                IsVisible = gameObject.IsVisible,
                Transform =
                {
                    Position = gameObject.Transform.Position,
                    Rotation = gameObject.Transform.Rotation,
                    Scale = gameObject.Transform.Scale,
                }
            };

            foreach (var child in gameObject.Children)
                copy.AddChild(Copy(child));

            foreach (var component in gameObject.Components)
                copy.AddComponent(GameObjectComponent.Copy(component));

            return copy;
        }
        public static IGameObject Find(string name)
        {
            return Singletons.SceneManager.OpenedScene.GameObjects.Find(name);
        }
    }
}