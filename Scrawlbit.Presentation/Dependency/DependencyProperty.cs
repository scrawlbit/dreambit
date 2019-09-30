using System.Windows;

namespace Scrawlbit.Presentation.Dependency
{
    public class DependencyProperty<TProperty>
    {
        private readonly DependencyProperty _property;
        
        public DependencyProperty(string propertyName, DependencyProperty property)
        {
            PropertyName = propertyName;
            _property = property;
        }

        public string PropertyName { get; private set; }

        public TProperty Get(DependencyObject owner)
        {
            return (TProperty) owner.GetValue(_property);
        }
        public void Set(DependencyObject owner, TProperty value)
        {
            owner.SetValue(_property, value);
        }

        public static implicit operator DependencyProperty(DependencyProperty<TProperty> propety)
        {
            return propety._property;
        }
    }

    public class DependencyProperty<TOwner, TProperty> where TOwner : DependencyObject
    {
        private readonly DependencyProperty<TProperty> _property;

        public DependencyProperty(string propertyName, DependencyProperty property)
        {
            _property = new DependencyProperty<TProperty>(propertyName, property);
        }

        public string PropertyName
        {
            get { return _property.PropertyName; }
        }
        public TProperty Get(TOwner owner)
        {
            return _property.Get(owner);
        }
        public void Set(TOwner owner, TProperty value)
        {
            _property.Set(owner, value);
        }

        public static implicit operator DependencyProperty(DependencyProperty<TOwner, TProperty> propety)
        {
            return propety._property;
        }
    }
}