using DreamBit.Game.Elements.Components;

namespace DreamBit.Game.Tests.Implementations.Elements
{
    public class GameObjectComponentImplementation : GameObjectComponent
    {
        public int StartCount;

        protected internal override void Start()
        {
            StartCount++;
        }
    }
}