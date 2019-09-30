using Microsoft.Xaml.Behaviors;
using System.Linq;
using System.Windows;

namespace ScrawlBit.Presentation.Helpers
{
    public static class InteractionHelper
    {
        public static T GetBehavior<T>(this DependencyObject element) where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(element);
            return behaviors.OfType<T>().SingleOrDefault();
        }

        public static bool HasBehavior<T>(this DependencyObject obj) where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(obj);
            return behaviors.OfType<T>().Any();
        }
        public static bool HasBehavior<T>(this DependencyObject obj, out T behavior) where T : Behavior
        {
            behavior = obj.GetBehavior<T>();
            return behavior != null;
        }

        public static void AddBehavior<T>(this DependencyObject element, T behavior) where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(element);
            behaviors.Add(behavior);
        }
        public static T AddBehavior<T>(this DependencyObject obj) where T : Behavior, new()
        {
            var newBehavior = new T();
            obj.AddBehavior(newBehavior);

            return newBehavior;
        }

        public static T FindBehaviorInChildren<T>(this DependencyObject parent) where T : Behavior
        {
            foreach (var child in parent.GetChildren())
            {
                var behavior = child.GetBehavior<T>() ?? child.FindBehaviorInChildren<T>();
                if (behavior != null)
                    return behavior;
            }

            return null;
        }
        public static T FindBehaviorInParent<T>(this DependencyObject child) where T : Behavior
        {
            var parent = child.GetParent();
            
            return parent?.GetBehavior<T>() ?? parent?.FindBehaviorInParent<T>();
        }
    }
}