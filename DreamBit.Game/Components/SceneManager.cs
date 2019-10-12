using System;
using System.Collections.Generic;
using DreamBit.Game.Content;
using DreamBit.Game.Elements;

namespace DreamBit.Game.Components
{
    internal class SceneManager : ISceneManager
    {
        private readonly IContentManager _contentManager;
        private readonly IContentReferenceManager _contentReferenceManager;

        public SceneManager(IContentManager contentManager, IContentReferenceManager contentReferenceManager)
        {
            _contentManager = contentManager;
            _contentReferenceManager = contentReferenceManager;
        }

        public Scene OpenedScene { get; private set; }

        public void Load(Guid fileId)
        {
            Load(_contentManager.Load<Scene>(fileId));
        }
        public void Load(string assetName)
        {
            Load(_contentManager.Load<Scene>(assetName));
        }
        private void Load(Scene scene)
        {
            OpenedScene = scene;
            ResolveContentReferences(OpenedScene.GameObjects);
            OpenedScene.Initialize();
        }

        private void ResolveContentReferences(IEnumerable<IGameObject> gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                foreach (var component in gameObject.Components)
                    _contentReferenceManager.Resolve(component);

                ResolveContentReferences(gameObject.Children);
            }
        }
    }
}