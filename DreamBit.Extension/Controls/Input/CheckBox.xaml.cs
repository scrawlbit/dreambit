﻿using Scrawlbit.Presentation.Dependency;
using System.Windows;

namespace DreamBit.Extension.Controls.Input
{
    public partial class CheckBox
    {
        public delegate void CheckBoxEventArgs(object sender, bool oldValue, bool newValue);
        private static readonly DependencyProperty<CheckBox, bool> IsCheckedProperty;
        private bool _initialValue;

        static CheckBox()
        {
            var dependency = new DependencyRegistry<CheckBox>();

            IsCheckedProperty = dependency.Property(b => b.IsChecked);
        }
        public CheckBox()
        {
            InitializeComponent();
        }

        public event CheckBoxEventArgs Changed;
        public bool IsChecked
        {
            get { return IsCheckedProperty.Get(this); }
            set { IsCheckedProperty.Set(this, value); }
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _initialValue = Toggle.IsChecked ?? false;
        }
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            var currentValue = Toggle.IsChecked ?? false;

            if (currentValue != _initialValue)
                Changed?.Invoke(this, _initialValue, currentValue);
        }
    }
}
