using System;
using System.ComponentModel;

namespace Scrawlbit.Notification.State
{
    public class PropertyChangesTracker<T> : ChangesTracker<T>
    {
        public PropertyChangesTracker(Action changeNotified) : base(changeNotified) { }

        public override void Track(T oldValue, T newValue)
        {
            var oldObject = (INotifyPropertyChanged)oldValue;
            if (oldObject != null)
                oldObject.PropertyChanged -= OnPropertyChanged;

            var newObject = (INotifyPropertyChanged)newValue;
            if (newObject != null)
                newObject.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object s, PropertyChangedEventArgs e)
        {
            ChangeNotified();
        }
    }
}