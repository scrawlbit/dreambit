using System;
using System.Windows;
using System.Windows.Controls;

namespace Scrawlbit.Presentation.Data
{
    public sealed class TypeTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate template = null;

            if (item != null && container is FrameworkElement element)
            {
                Type type = item as Type ?? item.GetType();
                DataTemplateKey key = new DataTemplateKey(type);

                template = element.TryFindResource(key) as DataTemplate;
            }

            return template ?? base.SelectTemplate(item, container);
        }
    }
}
