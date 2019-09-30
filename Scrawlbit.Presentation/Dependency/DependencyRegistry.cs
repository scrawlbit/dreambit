using System;
using System.Linq.Expressions;
using System.Windows;
using ScrawlBit.Helpers;

namespace ScrawlBit.Presentation.Dependency
{
    public class DependencyRegistry<TOwner> where TOwner : DependencyObject
    {
        public DependencyProperty<TOwner, TProperty> Property<TProperty>(Expression<Func<TOwner, TProperty>> property, Action<TOwner> callback = null)
        {
            if (callback == null)
                return Property(property, (Action<TOwner, DependencyPropertyChangedEventArgs>)null);

            return Property(property, (s, e) => callback(s));
        }
        public DependencyProperty<TOwner, TProperty> Property<TProperty>(Expression<Func<TOwner, TProperty>> property, Action<TOwner, DependencyPropertyChangedEventArgs> callback)
        {
            var name = ((MemberExpression)property.Body).Member.Name;
            var propertyChangedCallback = callback != null ? (s, e) => callback((TOwner)s, e) : (PropertyChangedCallback)null;
            var metadata = new PropertyMetadata(default(TProperty), propertyChangedCallback);

            var dependency = DependencyProperty.Register(name, typeof(TProperty), typeof(TOwner), metadata);

            return new DependencyProperty<TOwner, TProperty>(name, dependency);
        }

        public DependencyProperty<TProperty> AttachedProperty<TProperty>(Func<DependencyObject, TProperty> getMethod)
        {
            return AttachedProperty<TProperty>(getMethod.Method.Name.Remove("Get"));
        }
        public DependencyProperty<TProperty> AttachedProperty<TProperty>(string propertyName)
        {
            return new DependencyProperty<TProperty>(
                propertyName,
                DependencyProperty.RegisterAttached(propertyName, typeof(TProperty), typeof(TOwner))
            );
        }
    }
}