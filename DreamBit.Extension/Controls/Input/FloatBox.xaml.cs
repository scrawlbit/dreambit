using DreamBit.Extension.Helpers;
using DreamBit.General.State;
using Microsoft.Xna.Framework;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Behaviors;
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
        private readonly RegexInputValidationBehavior _mask;
        private float _initialValue;
        private float _increment;
        private bool _hasPrecision;

        static FloatBox()
        {
            var dependency = new DependencyRegistry<FloatBox>();

            ValueProperty = dependency.Property(e => e.Value, e => e.OnValueChanged());
            IsReadOnlyProperty = dependency.Property(e => e.IsReadOnly);
        }
        public FloatBox()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            _mask = new RegexInputValidationBehavior();

            Input.AddBehavior(_mask);

            Increment = 1;
            HasPrecision = true;
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
        public float Increment
        {
            get => HasPrecision ? _increment : _increment.Truncate();
            set => _increment = value;
        }
        public bool HasPrecision
        {
            get => _hasPrecision;
            set
            {
                if (value == _hasPrecision)
                    return;

                _hasPrecision = value;
                _mask.RegularExpression = value ? @"^-?\d*\.?\d*$" : @"^-?\d*$";

                if (!value)
                    Value = Value.Truncate();
            }
        }

        private void OnValueChanged()
        {
            UpdateText();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsReadOnly)
                return;

            if (e.Key == Key.Up)
            {
                IncrementValue(Increment);
                UpdateText();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                IncrementValue(-Increment);
                UpdateText();
                e.Handled = true;
            }
            else if (e.KeyIs(Key.OemMinus, Key.Subtract))
            {
                if (Equals(Value, 0f))
                {
                    if (Input.Text.StartsWith("-"))
                        UpdateText(Input.Text.Substring(1));
                    else
                        UpdateText("-" + Input.Text);
                }
                else
                {
                    Value = -Value;
                    UpdateText();
                }

                e.Handled = true;
            }
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            float value;
            float.TryParse(Input.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

            Value = value;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _initialValue = Value;

            if (Mouse.LeftButton != MouseButtonState.Pressed && Mouse.RightButton != MouseButtonState.Pressed)
                Input.SelectAll();
        }
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (IsRotation)
                Value = MathHelper.WrapAngle(Value);

            UpdateText();

            if (Value != _initialValue)
                Changed?.Invoke(this, (_initialValue, Value));
        }

        private void UpdateText(string value = null)
        {
            value = value ?? Value.ToString(CultureInfo.InvariantCulture);

            Input.Text = value;

            if (Input.IsFocused)
                Input.CaretIndex = Input.Text.Length;
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
