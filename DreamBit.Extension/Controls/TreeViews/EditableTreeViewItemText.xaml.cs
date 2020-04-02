using DreamBit.General.State;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Dependency;
using Scrawlbit.Presentation.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DreamBit.Extension.Controls.TreeViews
{
    public partial class EditableTreeViewItemText : UserControl
    {
        public delegate void TextChangedEventArgs(object sender, ValueChangedEventArgs<string> e);
        private static readonly DependencyProperty<EditableTreeViewItemText, string> TextProperty;

        static EditableTreeViewItemText()
        {
            var dependency = new DependencyRegistry<EditableTreeViewItemText>();

            TextProperty = dependency.Property(e => e.Text);
        }
        public EditableTreeViewItemText()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        public event TextChangedEventArgs Changed;
        public string Text
        {
            get { return TextProperty.Get(this); }
            set { TextProperty.Set(this, value); }
        }
        private bool EditingMode
        {
            get { return EditingText.Visibility == Visibility.Visible; }
            set
            {
                DisplayText.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                EditingText.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var treeViewItem = this.ParentsUntil<TreeViewItem>();

            treeViewItem.KeyDown += OnTreeViewItemKeyDown;
            treeViewItem.Unselected += OnTreeViewItemUnselected;

            Loaded -= OnLoaded;
        }
        private void OnTreeViewItemUnselected(object sender, RoutedEventArgs e)
        {
            if (EditingMode)
                CancelEdition();
        }
        private void OnTreeViewItemKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;

            if (e.Key == Key.F2 && !EditingMode)
            {
                StartEditing();
                e.Handled = true;
            }

            if (e.Key == Key.Enter && EditingMode)
            {
                ApplyChanges();
                e.Handled = true;
            }

            if (e.Key == Key.Escape && EditingMode)
            {
                CancelEdition();
                e.Handled = true;
            }
        }

        private void StartEditing()
        {
            EditingMode = true;
            EditingText.Text = Text;
            EditingText.Focus();
            EditingText.SelectAll();
        }
        private void ApplyChanges()
        {
            if (!EditingText.Text.HasValue())
                return;

            var oldValue = Text;
            var newValue = EditingText.Text.Trim();

            Text = newValue;
            CancelEdition();

            if (oldValue != newValue)
                Changed?.Invoke(this, (oldValue, newValue));
        }
        private void CancelEdition()
        {
            EditingMode = false;
        }
    }
}
