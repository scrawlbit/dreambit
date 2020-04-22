using DreamBit.General.State;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Dependency;
using System.Windows;

namespace DreamBit.Extension.Controls.Input
{
    public partial class TextBox
    {
        public delegate void TextBoxEventHandler(TextBox sender, ValueChangedEventArgs<string> e);
        public static readonly DependencyProperty<TextBox, string> TextProperty;
        private string _initialText;

        static TextBox()
        {
            var registry = new DependencyRegistry<TextBox>();

            TextProperty = registry.Property(t => t.Text);
        }
        public TextBox()
        {
            InitializeComponent();
        }

        public event TextBoxEventHandler Changed;
        public string Text
        {
            get => TextProperty.Get(this);
            set => TextProperty.Set(this, value);
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _initialText = Text;
        }
        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            Text = Text?.Trim().IfEmptyThenNull();

            if (Text != _initialText)
                Changed?.Invoke(this, (_initialText, Text));
        }
    }
}
