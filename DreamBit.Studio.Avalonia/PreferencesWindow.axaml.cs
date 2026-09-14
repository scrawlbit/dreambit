using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DreamBit.Studio;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Preferências do editor: pasta da engine, atalhos de teclado configuráveis, e
    /// exportar/importar tudo para compartilhar com outros PCs. Persiste em <see cref="EditorPreferences"/>.
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private EditorPreferences _prefs;
        private readonly Dictionary<string, TextBox> _shortcutBoxes = new();

        public PreferencesWindow()
        {
            InitializeComponent();
            _prefs = EditorPreferences.Load();
            var box = this.FindControl<TextBox>("PathBox")!;
            box.Text = _prefs.EnginePath ?? string.Empty;
            box.PropertyChanged += (_, e) => { if (e.Property.Name == nameof(TextBox.Text)) UpdateStatus(); };
            UpdateStatus();
            BuildShortcuts();
        }

        // ---- atalhos ----

        private void BuildShortcuts()
        {
            var panel = this.FindControl<StackPanel>("ShortcutsPanel")!;
            panel.Children.Clear();
            _shortcutBoxes.Clear();

            foreach (var (action, label) in EditorPreferences.KnownActions)
            {
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("180,*") };
                grid.Children.Add(new TextBlock
                {
                    Text = label, Foreground = new SolidColorBrush(Color.Parse("#B9C0CE")),
                    VerticalAlignment = VerticalAlignment.Center
                });

                var tb = new TextBox
                {
                    Text = _prefs.GestureFor(action),
                    IsReadOnly = true,
                    Watermark = "pressione…",
                    [Grid.ColumnProperty] = 1
                };
                // Captura a combinação pressionada e a atribui ao campo.
                tb.KeyDown += (_, e) =>
                {
                    var g = Gestures.Format(e.KeyModifiers, e.Key);
                    if (!string.IsNullOrEmpty(g))
                    {
                        tb.Text = g;
                        e.Handled = true;
                    }
                };
                _shortcutBoxes[action] = tb;
                grid.Children.Add(tb);
                panel.Children.Add(grid);
            }
        }

        private void OnResetShortcuts(object? sender, RoutedEventArgs e)
        {
            _prefs.ResetShortcuts();
            foreach (var (action, box) in _shortcutBoxes)
                box.Text = _prefs.GestureFor(action);
        }

        private void CollectShortcuts()
        {
            foreach (var (action, box) in _shortcutBoxes)
                _prefs.Shortcuts[action] = string.IsNullOrWhiteSpace(box.Text) ? "" : box.Text!.Trim();
            _prefs.FillDefaults();
        }

        // ---- engine path ----

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
                    ? "Engine válida nesta pasta."
                    : "Não achei o DreamBit.Player nesta pasta.";
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

        // ---- exportar / importar ----

        private async void OnExport(object? sender, RoutedEventArgs e)
        {
            CollectShortcuts();
            _prefs.EnginePath = NormalizedPath();
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Exportar preferências",
                DefaultExtension = "json",
                SuggestedFileName = "dreambit-preferences",
                FileTypeChoices = new[] { new FilePickerFileType("Preferências DreamBit") { Patterns = new[] { "*.json" } } }
            });
            var path = file?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
            {
                _prefs.ExportTo(path);
                SetShareStatus($"Exportado para {path}");
            }
        }

        private async void OnImport(object? sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Importar preferências", AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("Preferências DreamBit") { Patterns = new[] { "*.json" } } }
            });
            var path = files.FirstOrDefault()?.TryGetLocalPath();
            if (string.IsNullOrEmpty(path))
                return;
            try
            {
                _prefs = EditorPreferences.ImportFrom(path);
                this.FindControl<TextBox>("PathBox")!.Text = _prefs.EnginePath ?? "";
                BuildShortcuts();
                UpdateStatus();
                SetShareStatus("Importado. Clique em Salvar para aplicar.");
            }
            catch
            {
                SetShareStatus("Arquivo inválido.");
            }
        }

        private void SetShareStatus(string text)
            => this.FindControl<TextBlock>("ShareStatus")!.Text = text;

        private string? NormalizedPath()
        {
            var path = this.FindControl<TextBox>("PathBox")!.Text;
            return string.IsNullOrWhiteSpace(path) ? null : path.Trim();
        }

        // ---- salvar / cancelar ----

        private void OnSave(object? sender, RoutedEventArgs e)
        {
            _prefs.EnginePath = NormalizedPath();
            CollectShortcuts();
            _prefs.Save();
            (Owner as MainWindow)?.ReloadKeymap();
            Close();
        }

        private void OnCancel(object? sender, RoutedEventArgs e) => Close();
    }
}
