using DreamBit.Game.Elements;
using DreamBit.General.State;
using static DreamBit.General.State.StateHelper;

namespace DreamBit.Extension.Helpers
{
    public static class GameObjectComponentCollectionHelper
    {
        public static IStateCommand Add(this Target<IGameObjectComponentCollection> target, GameObjectComponent component, string stateDescription)
        {
            IGameObjectComponentCollection source = target.Object;

            return new StateCommand
            {
                Description = stateDescription,
                Do = () => source.Add(component),
                Undo = () => source.Remove(component)
            };
        }
    }
}
