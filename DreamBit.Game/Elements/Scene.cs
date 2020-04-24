using DreamBit.Game.Drawing;
using DreamBit.Game.Elements.Components;
using System.Collections.Generic;
using System.Linq;

namespace DreamBit.Game.Elements
{
    public sealed class Scene
    {
        public Scene()
        {
            Objects = new GameObjectCollection();
        }

        public IGameObjectCollection Objects { get; }

        public IEnumerable<Camera> GetCameras()
        {
            return Objects.SelectMany(o => o.Components).OfType<Camera>();
        }

        public void Preview(IContentDrawer drawer)
        {
            for (int i = 0; i < Objects.Count; i++)
                Objects[i].Preview(drawer);
        }
    }
}
