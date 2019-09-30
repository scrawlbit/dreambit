using System;

namespace ScrawlBit.Notification.State
{
    public abstract class ChangesTracker<T>
    {
        protected readonly Action ChangeNotified;

        protected ChangesTracker(Action changeNotified)
        {
            ChangeNotified = changeNotified;
        }

        public abstract void Track(T oldValue, T newValue);
    }
}