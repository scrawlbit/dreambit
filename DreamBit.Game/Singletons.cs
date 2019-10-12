using DreamBit.Game.Components;

namespace DreamBit.Game
{
    internal static class Singletons
    {
        private static ISceneManager _sceneManager;

        internal static ISceneManager SceneManager => Get(ref _sceneManager);

        private static T Get<T>(ref T variable) where T : class
        {
            return variable ?? BaseGame.Container.Inject(out variable);
        }
    }
}