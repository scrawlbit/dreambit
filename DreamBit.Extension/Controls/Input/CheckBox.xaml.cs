using DreamBit.General.State;
using Scrawlbit.Presentation.Dependency;
using System.Windows;

namespace DreamBit.Extension.Controls.Input
{
    public partial class CheckBox
    {
        public delegate void CheckBoxEventHandler(CheckBox sender, ValueChangedEventArgs<bool?> e);
        public static readonly DependencyProperty<CheckBox, bool?> IsCheckedProperty;
        private bool? _initialValue;

        static CheckBox()
        {
            var dependency = new DependencyRegistry<CheckBox>();

            IsCheckedProperty = dependency.Property(b => b.IsChecked);
        }
        public CheckBox()
        {
            InitializeComponent();
        }

        public event CheckBoxEventHandler Changed;
        public bool? IsChecked
        {
            get { return IsCheckedProperty.Get(this); }
            set { IsCheckedProperty.Set(this, value); }
        }
        public bool IsThreeState
        {
            get => Toggle.IsThreeState;
            set => Toggle.IsThreeState = value;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _initialValue = Toggle.IsChecked;
        }
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (IsChecked != _initialValue)
                Changed?.Invoke(this, (_initialValue, IsChecked));
        }
    }
}
