using System.Windows;

namespace ScrawlBit.Presentation.Dependency
{
    public class EventRegistry<TOwner> where TOwner : UIElement
    {
        public RoutedEvent<TOwner> Routed(string name)
        {
            var routedEvent = EventManager.RegisterRoutedEvent(name, RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TOwner));

            return new RoutedEvent<TOwner>(routedEvent); 
        }
    }
}