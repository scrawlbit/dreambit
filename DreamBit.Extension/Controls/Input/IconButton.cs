using Scrawlbit.Presentation.Dependency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DreamBit.Extension.Controls.Input
{
    public class IconButton : Button
    {
        public static readonly DependencyProperty<IconButton, ImageSource> IconProperty;
        public static readonly DependencyProperty<IconButton, CornerRadius> RadiusProperty;
        public static readonly DependencyProperty<IconButton, bool> IsCheckedProperty;
        public static readonly DependencyProperty<IconButton, SolidColorBrush> MouseOverColorBrushProperty;
        public static readonly DependencyProperty<IconButton, SolidColorBrush> PressedColorBrushProperty;
        public static readonly DependencyProperty<IconButton, SolidColorBrush> CheckedBrushProperty;

        static IconButton()
        {
            var dependency = new DependencyRegistry<IconButton>();

            IconProperty = dependency.Property(b => b.Icon);
            RadiusProperty = dependency.Property(b => b.Radius);
            IsCheckedProperty = dependency.Property(b => b.IsChecked);
            MouseOverColorBrushProperty = dependency.Property(b => b.MouseOverColorBrush);
            PressedColorBrushProperty = dependency.Property(b => b.PressedColorBrush);
            CheckedBrushProperty = dependency.Property(b => b.CheckedBrush);
        }

        public ImageSource Icon
        {
            get { return IconProperty.Get(this); }
            set { IconProperty.Set(this, value); }
        }
        public CornerRadius Radius
        {
            get { return RadiusProperty.Get(this); ; }
            set { RadiusProperty.Set(this, value); }
        }
        public bool IsChecked
        {
            get { return IsCheckedProperty.Get(this); ; }
            set { IsCheckedProperty.Set(this, value); }
        }
        public SolidColorBrush MouseOverColorBrush
        {
            get { return MouseOverColorBrushProperty.Get(this); ; }
            set { MouseOverColorBrushProperty.Set(this, value); }
        }
        public SolidColorBrush PressedColorBrush
        {
            get { return PressedColorBrushProperty.Get(this); ; }
            set { PressedColorBrushProperty.Set(this, value); }
        }
        public SolidColorBrush CheckedBrush
        {
            get { return CheckedBrushProperty.Get(this); ; }
            set { CheckedBrushProperty.Set(this, value); }
        }
    }
}
