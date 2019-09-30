using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ScrawlBit.Presentation.Behaviors
{
    public class RegexInputValidationBehavior : Behavior<TextBox>
    {
        private string _regularExpression;
        private Regex _regex;
        private int _minLength;

        public string RegularExpression
        {
            get { return _regularExpression; }
            set
            {
                if (value == _regularExpression) return;
                _regularExpression = value;
                _regex = new Regex(value);
            }
        }
        public int MinLength
        {
            get { return _minLength; }
            set { _minLength = Math.Abs(value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;

            base.OnDetaching();
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var text = AssociatedObject.Text;
            var selectedText = AssociatedObject.SelectedText;
            var caretIndex = AssociatedObject.CaretIndex;

            var selectedTextStartIndex = text.IndexOf(selectedText);
            if (caretIndex > selectedTextStartIndex)
                caretIndex -= selectedText.Length;

            var newText = text.Remove(caretIndex, selectedText.Length).Insert(caretIndex, e.Text);

            e.Handled = !IsInputValid(newText);
        }

        private bool IsInputValid(string text)
        {
            return _regex.IsMatch(text);
        }
    }
}