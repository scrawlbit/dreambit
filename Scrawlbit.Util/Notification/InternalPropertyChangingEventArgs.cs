namespace ScrawlBit.Notification
{
    internal class InternalPropertyChangingEventArgs : System.ComponentModel.PropertyChangingEventArgs
    {
        public InternalPropertyChangingEventArgs(string propertyName, object oldValue, object newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}