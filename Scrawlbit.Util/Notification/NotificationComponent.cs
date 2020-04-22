using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Scrawlbit.Notification
{
    public class NotificationComponent : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private readonly object _obj;

        public NotificationComponent(object obj)
        {
            _obj = obj;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public void OnPropertyChanging<T>(T oldValue, T newValue, string propertyName)
        {
            PropertyChanging?.Invoke(_obj, new InternalPropertyChangingEventArgs(propertyName, oldValue, newValue));
        }
        public void OnPropertyChanging<T, TProperty>(TProperty oldValue, TProperty newValue, Expression<Func<T, TProperty>> property)
        {
            OnPropertyChanging(oldValue, newValue, ((MemberExpression)property.Body).Member.Name);
        }
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(_obj, new PropertyChangedEventArgs(propertyName));
        }
        public void OnPropertyChanged<T, TProperty>(Expression<Func<T, TProperty>> property)
        {
            OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        public bool Set<T>(ref T variable, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(value, variable))
            {
                OnPropertyChanging(variable, value, propertyName);
                variable = value;
                OnPropertyChanged(propertyName);

                return true;
            }

            return false;
        }
    }
}