using DreamBit.General.State;
using System.Windows;

namespace DreamBit.Extension.Helpers
{
    public static class ControlHelper
    {
        #region IsSelected

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(false)
        );

        public static bool GetIsSelected(UIElement element)
        {
            return (bool)element.GetValue(IsSelectedProperty);
        }
        public static void SetIsSelected(UIElement element, bool value)
        {
            element.SetValue(IsSelectedProperty, value);
        }

        #endregion

        #region IsSelectionActive

        public static readonly DependencyProperty IsSelectionActiveProperty = DependencyProperty.RegisterAttached(
            "IsSelectionActive",
            typeof(bool),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(false)
        );

        public static bool GetIsSelectionActive(UIElement element)
        {
            return (bool)element.GetValue(IsSelectionActiveProperty);
        }
        public static void SetIsSelectionActive(UIElement element, bool value)
        {
            element.SetValue(IsSelectionActiveProperty, value);
        }

        #endregion

        #region ContentTemplate

        public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.RegisterAttached(
            "ContentTemplate",
            typeof(FrameworkTemplate),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(null)
        );

        public static bool GetContentTemplate(UIElement element)
        {
            return (bool)element.GetValue(ContentTemplateProperty);
        }
        public static void SetContentTemplate(UIElement element, bool value)
        {
            element.SetValue(ContentTemplateProperty, value);
        }

        #endregion

        #region TextChanged

        public delegate void TextChangedEventHandler(object sender, ValueChangedEventArgs<string> args);

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
            "TextChanged",
            RoutingStrategy.Bubble,
            typeof(TextChangedEventHandler),
            typeof(ControlHelper)
        );

        public static void AddTextChangedHandler(UIElement element, RoutedEventHandler handler)
        {
            element.AddHandler(TextChangedEvent, handler);
        }
        public static void RemoveTextChangedHandler(UIElement element, RoutedEventHandler handler)
        {
            element.RemoveHandler(TextChangedEvent, handler);
        }


        #endregion
    }
}