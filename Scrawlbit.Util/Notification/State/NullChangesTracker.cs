namespace ScrawlBit.Notification.State
{
    public class NullChangesTracker<T> : ChangesTracker<T>
    {
        public NullChangesTracker() : base(null)
        {
        }

        public override void Track(T oldValue, T newValue)
        {
        }
    }
}