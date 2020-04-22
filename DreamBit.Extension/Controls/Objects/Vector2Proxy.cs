using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Presentation.Dependency;
using System.Windows;

namespace DreamBit.Extension.Controls.Objects
{
    internal class Vector2Proxy : FrameworkElement
    {
        public static readonly DependencyProperty<Vector2Proxy, Vector2> ValueProperty;
        public static readonly DependencyProperty<Vector2Proxy, float> XProperty;
        public static readonly DependencyProperty<Vector2Proxy, float> YProperty;

        static Vector2Proxy()
        {
            var registry = new DependencyRegistry<Vector2Proxy>();

            ValueProperty = registry.Property(v => v.Value, v => v.OnValueChanged());
            XProperty = registry.Property(v => v.X, v => v.OnXChanged());
            YProperty = registry.Property(v => v.Y, v => v.OnYChanged());
        }
        public Vector2Proxy()
        {
            Visibility = Visibility.Collapsed;
        }

        public Vector2 Value
        {
            get => ValueProperty.Get(this);
            set => ValueProperty.Set(this, value);
        }
        public float X
        {
            get => XProperty.Get(this);
            set => XProperty.Set(this, value);
        }
        public float Y
        {
            get => YProperty.Get(this);
            set => YProperty.Set(this, value);
        }

        private void OnValueChanged()
        {
            if (X != Value.X) X = Value.X;
            if (Y != Value.Y) Y = Value.Y;
        }
        private void OnXChanged()
        {
            if (X != Value.X)
                Value = Value.Set(x: X);
        }
        private void OnYChanged()
        {
            if (Y != Value.Y)
                Value = Value.Set(y: Y);
        }
    }
}
