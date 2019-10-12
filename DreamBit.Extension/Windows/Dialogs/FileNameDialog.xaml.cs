using System;
using System.Windows;
using System.Windows.Input;
using Scrawlbit.Helpers;

namespace DreamBit.Extension.Windows.Dialogs
{
    public partial class FileNameDialog
    {
        public FileNameDialog()
        {
            InitializeComponent();
        }

        public event Action<string> FileNameInformed;

        public void Open(string title)
        {
            Title = title;
            ShowModal();
        }

        private void OnTextKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                NotifyFileNameInformed();
        }
        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            NotifyFileNameInformed();
        }

        private void NotifyFileNameInformed()
        {
            if (!FileName.Text.HasValue())
                return;

            string fileName = FileName.Text.Trim();

            FileNameInformed?.Invoke(fileName);
            Close();
        }
    }
}
