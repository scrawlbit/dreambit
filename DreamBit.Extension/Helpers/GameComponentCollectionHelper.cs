using DreamBit.Game.Elements;
using DreamBit.General.State;
using static DreamBit.General.State.StateHelper;

namespace DreamBit.Extension.Helpers
{
    public static class GameComponentCollectionHelper
    {
        public static IStateCommand Add(this Target<IGameComponentCollection> target, GameComponent component, string stateDescription)
        {
            IGameComponentCollection source = target.Object;

            return new StateCommand
            {
                Description = stateDescription,
                Do = () => source.Add(component),
                Undo = () => source.Remove(component)
            };
        }
    }
}
