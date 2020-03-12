using System.Collections.Generic;

namespace DreamBit.General.State
{
    public static class StateHelper
    {
        public static Target<T> State<T>(this T target)
        {
            return new Target<T> { Object = target };
        }

        public static IStateCommand Add<T>(this Target<ICollection<T>> target, T item, string stateDescription)
        {
            ICollection<T> source = target.Object;

            return new StateCommand
            {
                Description = stateDescription,
                Do = () => source.Add(item),
                Undo = () => source.Remove(item)
            };
        }

        #region Target

        public class Target<T>
        {
            public T Object { get; internal set; }
        }

        #endregion
    }
}
