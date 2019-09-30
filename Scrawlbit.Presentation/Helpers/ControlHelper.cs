using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Scrawlbit.Presentation.Helpers
{
    public static class ControlHelper
    {
        public static IEnumerable<DependencyObject> GetChildren(this DependencyObject element)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                yield return VisualTreeHelper.GetChild(element, i);
        }
        public static DependencyObject GetParent(this DependencyObject child)
        {
            return VisualTreeHelper.GetParent(child);
        }
        public static T ParentsUntil<T>(this DependencyObject child, Func<DependencyObject, bool> stopFunction = null) where T : DependencyObject
        {
            var parentObject = child.GetParent();
            if (parentObject == null || stopFunction != null && stopFunction(parentObject))
                return null;

            return parentObject as T ?? parentObject.ParentsUntil<T>(stopFunction);
        }

        public static IEnumerable<T> FindChildren<T>(this DependencyObject reference) where T : class
        {
            var found = new List<T>();

            foreach (var child in reference.GetChildren())
            {
                var t = child as T;

                if (t != null)
                    found.Add(t);
                else
                    found.AddRange(child.FindChildren<T>());
            }

            return found;
        }
        public static T FindChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            foreach (var child in parent.GetChildren())
            {
                var t = child as T ?? child.FindChild<T>();
                if (t != null)
                    return t;
            }

            return null;
        }
        public static T FindChild<T>(this DependencyObject parent, string childName) where T : FrameworkElement
        {
            foreach (var child in parent.GetChildren())
            {
                if ((child as T)?.Name == childName)
                    return (T)child;

                var childOfChild = FindChild<T>(child, childName);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        public static void UpdateBindingSources(DependencyObject obj, params DependencyProperty[] properties)
        {
            foreach (var depProperty in properties)
            {
                var be = BindingOperations.GetBindingExpression(obj, depProperty);
                if (be != null) be.UpdateSource();
            }

            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                var childObject = VisualTreeHelper.GetChild(obj, i);
                UpdateBindingSources(childObject, properties);
            }
        }

        public static T TryFindFromPoint<T>(UIElement reference, Point point) where T : DependencyObject
        {
            var element = reference.InputHitTest(point)
                as DependencyObject;
            if (element == null) return null;
            if (element is T) return (T)element;
            return ParentsUntil<T>(element);
        }
    }
}