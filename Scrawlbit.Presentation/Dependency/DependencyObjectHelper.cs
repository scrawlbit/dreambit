using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using Scrawlbit.Helpers;

namespace Scrawlbit.Presentation.Dependency
{
    public static class DependencyObjectHelper
    {
        public static void Binding<TSource, TTarget, TProperty>(
            this TTarget target,
            DependencyProperty<TTarget, TProperty> property,
            TSource source,
            Expression<Func<TSource, TProperty>> path,
            BindingMode mode = BindingMode.OneWay)
            where TTarget : DependencyObject
        {
            target.Binding((DependencyProperty)property, source, path, mode);
        }

        public static void Binding<TSource, TProperty>(
            this DependencyObject target,
            DependencyProperty property,
            TSource source,
            Expression<Func<TSource, TProperty>> path,
            BindingMode mode = BindingMode.OneWay)
        {
            BindingOperations.SetBinding(target, property, new Binding(ExpressionHelper.GetExpressionText(path))
            {
                Source = source,
                Mode = mode
            });
        }

        public static void Binding(this DependencyObject target, DependencyProperty property, object source)
        {
            BindingOperations.SetBinding(target, property, new Binding
            {
                Source = source
            });
        }

        public static void OnPropertyChanged<T, TProperty>(T obj, Expression<Func<T, TProperty>> property) where T : ComponentObject
        {
            obj.NotificationComponent.OnPropertyChanged(property);
        }
    }
}