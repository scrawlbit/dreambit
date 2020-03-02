using System.Collections.Generic;
using DreamBit.Game.Content;
using DreamBit.Game.Helpers;
using Newtonsoft.Json;

namespace DreamBit.Game.Elements
{
    public sealed class Scene : IContent
    {
        public Scene()
        {
            GameObjects = new List<IGameObject>();
        }

        [JsonProperty("Objects")]
        public List<IGameObject> GameObjects { get; }

        internal void Initialize()
        {
            GameObjects.InitializeAll();
        }
        internal void Update()
        {
            UpdateAll(GameObjects);
        }
        internal void PostUpdate()
        {
            PostUpdateAll(GameObjects);
        }
        internal void Draw()
        {
            DrawAll(GameObjects);
        }

        private static void UpdateAll(IReadOnlyList<IGameObject> gameObjects)
        {
            for (var g = 0; g < gameObjects.Count; g++)
                Update(gameObjects[g]);
        }
        private static void PostUpdateAll(IReadOnlyList<IGameObject> gameObjects)
        {
            for (var g = 0; g < gameObjects.Count; g++)
                PostUpdate(gameObjects[g]);
        }
        private static void DrawAll(IReadOnlyList<IGameObject> gameObjects)
        {
            for (var g = 0; g < gameObjects.Count; g++)
                Draw(gameObjects[g]);
        }

        private static void Update(IGameObject gameObject)
        {
            if (!gameObject.IsVisible)
                return;

            for (var i = 0; i < gameObject.Components.Count; i++)
            {
                var component = gameObject.Components[i];

                component.Initialize(gameObject);
                component.Update();
            }

            UpdateAll(gameObject.Children);
        }
        private static void PostUpdate(IGameObject gameObject)
        {
            if (!gameObject.IsVisible)
                return;

            for (var i = 0; i < gameObject.Components.Count; i++)
            {
                var component = gameObject.Components[i];

                if (component.Started)
                    component.PostUpdate();
            }

            PostUpdateAll(gameObject.Children);
        }
        private static void Draw(IGameObject gameObject)
        {
            if (!gameObject.IsVisible)
                return;

            gameObject.Transform.ValidateTransformations();

            for (var i = 0; i < gameObject.Components.Count; i++)
            {
                var component = gameObject.Components[i];

                if (component.Started)
                    component.Draw();
            }

            DrawAll(gameObject.Children);
        }
    }
}