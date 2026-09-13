using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DreamBit.Studio.ViewModels;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Seletor de atlas (Avalonia): abre um PNG em 1:1, arrasta um retângulo de recorte e
    /// o usa como carimbo — "Usar como carimbo" liga o modo carimbo (clicar na cena posiciona)
    /// ou "Carimbar no centro" coloca um de imediato. O 1:1 faz as coordenadas do canvas
    /// serem os pixels da imagem.
    /// </summary>
    public partial class AtlasPicker : Window
    {
        private readonly EditorViewModel _editor;
        private string? _texturePath;
        private Point _start;
        private bool _dragging;
        private XnaRectangle _selection;

        public AtlasPicker() : this(new EditorViewModel()) { } // designer

        public AtlasPicker(EditorViewModel editor)
        {
            InitializeComponent();
            _editor = editor;
        }

        private async void OnOpen(object? sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Abrir atlas/spritesheet",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Imagens") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" } }
                }
            });

            var path = files.FirstOrDefault()?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
                Load(path);
        }

        private void Load(string path)
        {
            try
            {
                var image = this.FindControl<Image>("AtlasImage")!;
                var canvas = this.FindControl<Canvas>("PickCanvas")!;
                var bmp = new Bitmap(path);

                image.Source = bmp;
                canvas.Width = bmp.PixelSize.Width;
                canvas.Height = bmp.PixelSize.Height;
                _texturePath = path;

                this.FindControl<Rectangle>("SelectionRect")!.IsVisible = false;
                _selection = XnaRectangle.Empty;
                SetButtons(false);
                this.FindControl<TextBlock>("Info")!.Text =
                    $"{System.IO.Path.GetFileName(path)} — {bmp.PixelSize.Width}×{bmp.PixelSize.Height}px. Arraste um retângulo.";
            }
            catch (Exception ex)
            {
                this.FindControl<TextBlock>("Info")!.Text = "Falha ao abrir: " + ex.Message;
            }
        }

        private void OnCanvasPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_texturePath == null)
                return;

            var canvas = this.FindControl<Canvas>("PickCanvas")!;
            _start = e.GetPosition(canvas);
            _dragging = true;

            var rect = this.FindControl<Rectangle>("SelectionRect")!;
            Canvas.SetLeft(rect, _start.X);
            Canvas.SetTop(rect, _start.Y);
            rect.Width = 0;
            rect.Height = 0;
            rect.IsVisible = true;
            e.Pointer.Capture(canvas);
        }

        private void OnCanvasMoved(object? sender, PointerEventArgs e)
        {
            if (!_dragging)
                return;

            var canvas = this.FindControl<Canvas>("PickCanvas")!;
            var p = e.GetPosition(canvas);
            double x = Math.Min(p.X, _start.X);
            double y = Math.Min(p.Y, _start.Y);

            var rect = this.FindControl<Rectangle>("SelectionRect")!;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            rect.Width = Math.Abs(p.X - _start.X);
            rect.Height = Math.Abs(p.Y - _start.Y);
        }

        private void OnCanvasReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_dragging)
                return;

            _dragging = false;
            e.Pointer.Capture(null);

            var canvas = this.FindControl<Canvas>("PickCanvas")!;
            var rect = this.FindControl<Rectangle>("SelectionRect")!;

            int x = (int)Math.Round(Canvas.GetLeft(rect));
            int y = (int)Math.Round(Canvas.GetTop(rect));
            int w = (int)Math.Round(rect.Width);
            int h = (int)Math.Round(rect.Height);

            x = Math.Clamp(x, 0, (int)canvas.Width);
            y = Math.Clamp(y, 0, (int)canvas.Height);
            w = Math.Clamp(w, 0, (int)canvas.Width - x);
            h = Math.Clamp(h, 0, (int)canvas.Height - y);

            if (w < 2 || h < 2)
            {
                rect.IsVisible = false;
                SetButtons(false);
                return;
            }

            _selection = new XnaRectangle(x, y, w, h);
            SetButtons(true);
            this.FindControl<TextBlock>("Info")!.Text = $"Recorte: {x}, {y} · {w}×{h}px";
        }

        private void OnUseStamp(object? sender, RoutedEventArgs e)
        {
            if (_texturePath != null && _selection is { Width: > 0, Height: > 0 })
            {
                _editor.SetCurrentStamp(_texturePath, _selection);
                this.FindControl<TextBlock>("Info")!.Text =
                    "Carimbo definido — clique na cena para posicionar (modo carimbo ligado).";
            }
        }

        private void OnStampCenter(object? sender, RoutedEventArgs e)
        {
            if (_texturePath != null && _selection is { Width: > 0, Height: > 0 })
                _editor.StampFromAtlas(_texturePath, _selection, _editor.Camera.Position);
        }

        private void SetButtons(bool enabled)
        {
            this.FindControl<Button>("UseButton")!.IsEnabled = enabled;
            this.FindControl<Button>("StampCenterButton")!.IsEnabled = enabled;
        }
    }
}
