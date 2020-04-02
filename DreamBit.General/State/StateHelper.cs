using Scrawlbit.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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

        public static IStateCommand SetProperty<T, TProperty>(this Target<T> target, Expression<Func<T, TProperty>> property, TProperty oldValue, TProperty newValue, string stateDescription)
        {
            T obj = target.Object;
            PropertyInfo p = (PropertyInfo)((MemberExpression)property.Body).Member;

            return new StateCommand
            {
                Description = stateDescription,
                Do = () => obj.SetProperty(p, newValue),
                Undo = () => obj.SetProperty(p, oldValue)
            };
        }
        public static IStateCommand SetProperty<T, TProperty>(this Target<T> target, Expression<Func<T, TProperty>> property, ValueChangedEventArgs<TProperty> args, string stateDescription)
        {
            return SetProperty(target, property, args.OldValue, args.NewValue, stateDescription);
        }

        #region Target

        public class Target<T>
        {
            public T Object { get; internal set; }
        }

        #endregion
    }
}
