using DreamBit.General.State;
using Microsoft.Xna.Framework;
using Scrawlbit.Presentation.Dependency;
using Scrawlbit.Presentation.Helpers;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace DreamBit.Extension.Controls.Input
{
    public partial class FloatBox
    {
        public delegate void FloatBoxEventHandler(FloatBox sender, ValueChangedEventArgs<float> e);
        private static readonly DependencyProperty<FloatBox, float> ValueProperty;
        private static readonly DependencyProperty<FloatBox, bool> IsReadOnlyProperty;
        private float _initialValue;

        static FloatBox()
        {
            var dependency = new DependencyRegistry<FloatBox>();

            ValueProperty = dependency.Property(e => e.Value, e => e.OnValueChanged());
            IsReadOnlyProperty = dependency.Property(e => e.IsReadOnly);
        }
        public FloatBox()
        {
            InitializeComponent();

            Increment = 1;
        }

        public event FloatBoxEventHandler Changed;
        public float Value
        {
            get => ValueProperty.Get(this);
            set => ValueProperty.Set(this, value);
        }
        public bool IsRotation { get; set; }
        public bool IsReadOnly
        {
            get => IsReadOnlyProperty.Get(this);
            set => IsReadOnlyProperty.Set(this, value);
        }
        public float Increment { get; set; }

        private void OnValueChanged()
        {
            UpdateEditingText();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsReadOnly)
                return;

            if (e.Key == Key.Up)
            {
                IncrementValue(Increment);
                UpdateEditingText();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                IncrementValue(-Increment);
                UpdateEditingText();
                e.Handled = true;
            }
            else if (e.KeyIs(Key.OemMinus, Key.Subtract))
            {
                if (Equals(Value, 0f))
                {
                    if (EditingText.Text.StartsWith("-"))
                        SetEditingText(EditingText.Text.Substring(1));
                    else
                        SetEditingText("-" + EditingText.Text);
                }
                else
                {
                    Value = -Value;
                    UpdateEditingText();
                }

                e.Handled = true;
            }
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            float value;
            float.TryParse(EditingText.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

            Value = value;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _initialValue = Value;

            if (Mouse.LeftButton != MouseButtonState.Pressed && Mouse.RightButton != MouseButtonState.Pressed)
                EditingText.SelectAll();
        }
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (IsRotation)
                Value = MathHelper.WrapAngle(Value);

            UpdateEditingText();

            if (Value != _initialValue)
                Changed?.Invoke(this, (_initialValue, Value));
        }

        private void UpdateEditingText()
        {
            SetEditingText(Value.ToString(CultureInfo.InvariantCulture));
        }
        private void SetEditingText(string value)
        {
            EditingText.Text = value;

            if (EditingText.IsFocused)
                EditingText.CaretIndex = EditingText.Text.Length;
        }

        private void IncrementValue(float value)
        {
            if (!IsRotation)
            {
                Value += value;
                return;
            }

            var degrees = Math.Round(MathHelper.ToDegrees(Value), 0) + value;

            Value = MathHelper.ToRadians((float)degrees);
        }
    }
}
