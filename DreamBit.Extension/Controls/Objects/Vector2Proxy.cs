using Microsoft.Xna.Framework;
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
            X = Value.X;
            Y = Value.Y;
        }
        private void OnXChanged()
        {
            Value = new Vector2(X, Value.Y);
        }
        private void OnYChanged()
        {
            Value = new Vector2(Value.X, Y);
        }
    }
}
