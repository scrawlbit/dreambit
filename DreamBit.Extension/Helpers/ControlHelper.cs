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
    }
}