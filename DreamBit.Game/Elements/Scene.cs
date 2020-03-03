using Scrawlbit.Collections;

namespace DreamBit.Game.Elements
{
    public sealed class Scene
    {
        public Scene()
        {
            Objects = new ExtendedObservableCollection<GameObject>();
        }

        public IObservableCollection<GameObject> Objects { get; }
    }
}
