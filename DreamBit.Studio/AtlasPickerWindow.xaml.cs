using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace DreamBit.Studio
{
    /// <summary>
    /// Seletor de atlas: abre um PNG em escala 1:1, deixa arrastar um retângulo de recorte
    /// e "carimba" na cena um sprite com aquele recorte (source rect). Mantém a janela aberta
    /// para carimbar vários recortes seguidos. O 1:1 dispensa conversão de escala: as
    /// coordenadas no canvas já são os pixels da imagem.
    /// </summary>
    public partial class AtlasPickerWindow : Window
    {
        private readonly Action<string, XnaRectangle> _onStamp;
        private string? _texturePath;
        private Point _start;
        private bool _dragging;
        private XnaRectangle _selection;

        public AtlasPickerWindow(Action<string, XnaRectangle> onStamp, string? initialTexture = null)
        {
            InitializeComponent();
            _onStamp = onStamp;
            if (!string.IsNullOrEmpty(initialTexture) && System.IO.File.Exists(initialTexture))
                Load(initialTexture);
        }

        private void OnOpen(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Imagens (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Abrir atlas/spritesheet"
            };
            if (dialog.ShowDialog(this) == true)
                Load(dialog.FileName);
        }

        private void Load(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            _texturePath = path;
            AtlasImage.Source = bitmap;
            PickCanvas.Width = bitmap.PixelWidth;
            PickCanvas.Height = bitmap.PixelHeight;

            SelectionRect.Visibility = Visibility.Collapsed;
            _selection = XnaRectangle.Empty;
            StampButton.IsEnabled = false;
            SelectionLabel.Text = $"{System.IO.Path.GetFileName(path)} — {bitmap.PixelWidth}×{bitmap.PixelHeight}px. Arraste um retângulo.";
        }

        private void OnCanvasDown(object sender, MouseButtonEventArgs e)
        {
            if (_texturePath == null)
                return;

            _start = e.GetPosition(PickCanvas);
            _dragging = true;
            PickCanvas.CaptureMouse();

            Canvas.SetLeft(SelectionRect, _start.X);
            Canvas.SetTop(SelectionRect, _start.Y);
            SelectionRect.Width = 0;
            SelectionRect.Height = 0;
            SelectionRect.Visibility = Visibility.Visible;
        }

        private void OnCanvasMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
                return;

            var p = e.GetPosition(PickCanvas);
            double x = Math.Min(p.X, _start.X);
            double y = Math.Min(p.Y, _start.Y);
            double w = Math.Abs(p.X - _start.X);
            double h = Math.Abs(p.Y - _start.Y);

            Canvas.SetLeft(SelectionRect, x);
            Canvas.SetTop(SelectionRect, y);
            SelectionRect.Width = w;
            SelectionRect.Height = h;
        }

        private void OnCanvasUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragging)
                return;

            _dragging = false;
            PickCanvas.ReleaseMouseCapture();

            int x = (int)Math.Round(Canvas.GetLeft(SelectionRect));
            int y = (int)Math.Round(Canvas.GetTop(SelectionRect));
            int w = (int)Math.Round(SelectionRect.Width);
            int h = (int)Math.Round(SelectionRect.Height);

            // Limita aos limites da imagem.
            x = Math.Clamp(x, 0, (int)PickCanvas.Width);
            y = Math.Clamp(y, 0, (int)PickCanvas.Height);
            w = Math.Clamp(w, 0, (int)PickCanvas.Width - x);
            h = Math.Clamp(h, 0, (int)PickCanvas.Height - y);

            if (w < 2 || h < 2)
            {
                SelectionRect.Visibility = Visibility.Collapsed;
                StampButton.IsEnabled = false;
                return;
            }

            _selection = new XnaRectangle(x, y, w, h);
            StampButton.IsEnabled = true;
            SelectionLabel.Text = $"Recorte: {x}, {y}  ·  {w}×{h}px";
        }

        private void OnStamp(object sender, RoutedEventArgs e)
        {
            if (_texturePath != null && _selection is { Width: > 0, Height: > 0 })
                _onStamp(_texturePath, _selection);
        }
    }
}
