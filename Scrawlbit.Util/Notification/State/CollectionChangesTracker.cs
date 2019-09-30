using System;
using System.Collections.Specialized;

namespace Scrawlbit.Notification.State
{
    public class CollectionChangesTracker<T> : ChangesTracker<T>
    {
        public CollectionChangesTracker(Action changeNotified) : base(changeNotified) { }

        public override void Track(T oldValue, T newValue)
        {
            var oldCollection = (INotifyCollectionChanged)oldValue;
            if (oldCollection != null)
                oldCollection.CollectionChanged -= OnCollectionChanged;

            var newCollection = (INotifyCollectionChanged)newValue;
            if (newCollection != null)
                newCollection.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object s, NotifyCollectionChangedEventArgs e)
        {
            ChangeNotified();
        }
    }
}