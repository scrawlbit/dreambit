using System.Windows;
using System.Windows.Documents;

namespace ScrawlBit.Presentation.DragAndDrop
{
    public abstract class DroppableAdorner : Adorner
    {
        private AdornerLayer _layer;

        protected DroppableAdorner(UIElement adornedElement) : base(adornedElement)
        {
            IsHitTestVisible = false;
        }

        public virtual void Update(DropType dropType, object[] data, object target)
        {
            if (_layer == null)
            {
                _layer = AdornerLayer.GetAdornerLayer(AdornedElement);
                _layer.Add(this);
            }

            _layer.Update(AdornedElement);
            Visibility = Visibility.Visible;
        }
        public virtual void Remove()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}