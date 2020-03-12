using DreamBit.Game.Elements;
using DreamBit.General.State;
using static DreamBit.General.State.StateHelper;

namespace DreamBit.Extension.Helpers
{
    public static class ExtendedStateHelper
    {
        public static IStateCommand Add(this Target<IGameObjectCollection> target, GameObject gameObject, string stateDescription)
        {
            IGameObjectCollection source = target.Object;

            return new StateCommand
            {
                Description = stateDescription,
                Do = () => source.Add(gameObject),
                Undo = () => source.Remove(gameObject)
            };
        }
    }
}
