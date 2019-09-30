using System.Windows;

namespace ScrawlBit.Presentation.Dependency
{
    public class RoutedEvent<TOwner> where TOwner : UIElement
    {
        private readonly RoutedEvent _routedEvent;

        internal RoutedEvent(RoutedEvent routedEvent)
        {
            _routedEvent = routedEvent;
        }

        public void Add(TOwner owner, RoutedEventHandler handler)
        {
            owner.AddHandler(_routedEvent, handler);
        }
        public void Remove(TOwner owner, RoutedEventHandler handler)
        {
            owner.RemoveHandler(_routedEvent, handler);
        }

        public void Raise(TOwner owner)
        {
            owner.RaiseEvent(new RoutedEventArgs(_routedEvent));
        }
    }
}