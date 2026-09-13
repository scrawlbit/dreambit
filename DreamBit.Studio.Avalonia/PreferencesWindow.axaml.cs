using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DreamBit.Studio;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Preferências do editor: define a pasta da engine (onde está o DreamBit.Player),
    /// usada para Rodar/Exportar. Persistida em <see cref="EditorPreferences"/>.
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private readonly EditorPreferences _prefs;

        public PreferencesWindow()
        {
            InitializeComponent();
            _prefs = EditorPreferences.Load();
            var box = this.FindControl<TextBox>("PathBox")!;
            box.Text = _prefs.EnginePath ?? string.Empty;
            box.PropertyChanged += (_, e) => { if (e.Property.Name == nameof(TextBox.Text)) UpdateStatus(); };
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var status = this.FindControl<TextBlock>("StatusText")!;
            var path = this.FindControl<TextBox>("PathBox")!.Text;

            if (string.IsNullOrWhiteSpace(path))
            {
                var auto = EngineLocator.FindRoot();
                status.Text = auto != null ? $"Auto: encontrada em {auto}" : "Auto: engine não encontrada.";
            }
            else
            {
                status.Text = EngineLocator.IsEngineRoot(path)
                    ? "✔ Engine válida nesta pasta."
                    : "✘ Não achei o DreamBit.Player nesta pasta.";
            }
        }

        private async void OnBrowse(object? sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { Title = "Pasta da engine (contém DreamBit.Player)", AllowMultiple = false });
            var path = folders.FirstOrDefault()?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
            {
                this.FindControl<TextBox>("PathBox")!.Text = path;
                UpdateStatus();
            }
        }

        private void OnSave(object? sender, RoutedEventArgs e)
        {
            var path = this.FindControl<TextBox>("PathBox")!.Text;
            _prefs.EnginePath = string.IsNullOrWhiteSpace(path) ? null : path.Trim();
            _prefs.Save();
            Close();
        }

        private void OnCancel(object? sender, RoutedEventArgs e) => Close();
    }
}
