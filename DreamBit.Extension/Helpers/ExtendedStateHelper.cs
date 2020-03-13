using DreamBit.Game.Elements;
using DreamBit.General.State;
using Scrawlbit.Helpers;
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
        public static IStateCommand Remove(this Target<IGameObjectCollection> target, GameObject gameObject, string stateDescription)
        {
            IGameObjectCollection source = target.Object;
            int index = source.IndexOf(gameObject);

            return new StateCommand
            {
                Description = stateDescription,
                Do = () => source.Remove(gameObject),
                Undo = () => source.Insert(index, gameObject)
            };
        }
    }
}
