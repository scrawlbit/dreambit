namespace ScrawlBit.Notification
{
    public class InternalPropertyChangedEventArgs : System.ComponentModel.PropertyChangedEventArgs
    {
        public InternalPropertyChangedEventArgs(string propertyName, object oldValue, object newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}