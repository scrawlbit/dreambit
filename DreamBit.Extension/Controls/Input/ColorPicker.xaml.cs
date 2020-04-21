using DreamBit.Extension.Helpers;
using Microsoft.Xna.Framework;
using Scrawlbit.Presentation.Dependency;
using System.Windows;
using System.Windows.Media;
using MonoGameColor = Microsoft.Xna.Framework.Color;

namespace DreamBit.Extension.Controls.Input
{
    public partial class ColorPicker
    {
        public delegate void ColorPickerEventHandler(ColorPicker sender, ColorPickerChangedEventArgs e);
        public static readonly DependencyProperty<ColorPicker, MonoGameColor> ColorProperty;
        private MonoGameColor _initialColor;

        static ColorPicker()
        {
            var dependency = new DependencyRegistry<ColorPicker>();

            ColorProperty = dependency.Property(p => p.Color, p => p.OnColorChanged());
        }
        public ColorPicker()
        {
            InitializeComponent();
        }

        public event ColorPickerEventHandler Changed;
        public MonoGameColor Color
        {
            get => ColorProperty.Get(this);
            set => ColorProperty.Set(this, value);
        }

        private void OnColorChanged()
        {
            UpdateViewValue();
        }

        private void OnColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (Picker.IsOpen && e.NewValue != null)
                Color = e.NewValue.Value.ToMonoGameColor();
        }
        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _initialColor = Color;
        }
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (Color != _initialColor)
                Changed?.Invoke(this, new ColorPickerChangedEventArgs(_initialColor, Color, Picker.SelectedColorText));
        }

        private void UpdateViewValue()
        {
            Picker.SelectedColor = Color.ToMediaColor();
        }
    }
}
