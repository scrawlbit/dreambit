using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DreamBit.Engine.Notification;
using DreamBit.Studio;
using DreamBit.Studio.ViewModels;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Seletor de atlas: abre um PNG 1:1, recorta vários carimbos nomeados (arrastar, clicar
    /// para auto-dimensionar pelo sprite, ou "Detectar tudo"), com zoom, pan (scroll do meio),
    /// mover o recorte, e persistência ao lado do PNG (&lt;atlas&gt;.stamps.json). Selecionar um
    /// carimbo e "Usar" liga o modo carimbo na cena (Esc sai).
    /// </summary>
    public partial class AtlasPicker : Window
    {
        private readonly EditorViewModel _editor;
        private string? _texturePath;
        private AtlasStampLibrary _library = new();
        private readonly ObservableCollection<StampVm> _stamps = new();
        private readonly List<Rectangle> _overlays = new();
        private List<XnaRectangle> _frames = new(); // sprites detectados (auto-dimensão)

        private XnaRectangle _selection;
        private double _zoom = 1;

        // Estados de ponteiro
        private Point _start;
        private bool _creating, _movingSel, _panning;
        private Point _moveGrab;
        private Point _panStart;
        private Vector _panOffset;

        public AtlasPicker() : this(new EditorViewModel()) { } // designer

        private List<string> _recentPaths = new();

        public AtlasPicker(EditorViewModel editor)
        {
            InitializeComponent();
            _editor = editor;
            this.FindControl<ListBox>("StampList")!.ItemsSource = _stamps;
            RefreshRecent();
            // Reutilizar: reabre o último atlas cortado.
            var last = _recentPaths.FirstOrDefault(System.IO.File.Exists);
            if (last != null)
                Load(last);
        }

        private void RefreshRecent()
        {
            _recentPaths = DreamBit.Studio.EditorPreferences.Load().RecentAtlases.ToList();
            var box = this.FindControl<ComboBox>("RecentBox")!;
            box.ItemsSource = _recentPaths.Select(System.IO.Path.GetFileName).ToList();
        }

        private void OnRecentSelected(object? sender, SelectionChangedEventArgs e)
        {
            var box = this.FindControl<ComboBox>("RecentBox")!;
            int i = box.SelectedIndex;
            if (i >= 0 && i < _recentPaths.Count && System.IO.File.Exists(_recentPaths[i]) && _recentPaths[i] != _texturePath)
                Load(_recentPaths[i]);
        }

        private void OnStampDoubleClick(object? sender, TappedEventArgs e) => OnUseSelected(sender, new RoutedEventArgs());

        private void OnStampKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) { OnDeleteSelected(sender, new RoutedEventArgs()); e.Handled = true; }
        }

        // ---------- carregar ----------

        private async void OnOpen(object? sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Abrir atlas/spritesheet",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("Imagens") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" } } }
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

                _selection = XnaRectangle.Empty;
                this.FindControl<Rectangle>("SelectionRect")!.IsVisible = false;
                AddEnabled(false);

                // Detecta os sprites (para clicar e auto-dimensionar).
                _frames = PngMask.TryLoad(path, out var opaque, out int w, out int h)
                    ? DreamBit.Engine.Rendering.FrameDetector.Detect(opaque, w, h)
                    : new List<XnaRectangle>();

                // Carrega os carimbos já salvos deste atlas.
                _library = AtlasStampLibrary.Load(path);
                RebuildStamps();

                this.FindControl<TextBlock>("Info")!.Text =
                    $"{System.IO.Path.GetFileName(path)} — {w}×{h}px · {_frames.Count} sprites detectados · {_library.Stamps.Count} carimbos salvos.";

                DreamBit.Studio.EditorPreferences.Load().PushRecentAtlas(path); // reutilizar depois
                RefreshRecent();
            }
            catch (Exception ex)
            {
                this.FindControl<TextBlock>("Info")!.Text = "Falha ao abrir: " + ex.Message;
            }
        }

        // ---------- carimbos (lista + overlays) ----------

        private void RebuildStamps()
        {
            _stamps.Clear();
            foreach (var s in _library.Stamps)
                _stamps.Add(new StampVm(s));
            DrawOverlays();
        }

        private void DrawOverlays()
        {
            var canvas = this.FindControl<Canvas>("PickCanvas")!;
            foreach (var r in _overlays)
                canvas.Children.Remove(r);
            _overlays.Clear();

            foreach (var s in _library.Stamps)
            {
                var r = new Rectangle
                {
                    Width = s.W, Height = s.H,
                    Stroke = new SolidColorBrush(Color.Parse("#7AF0B0")), StrokeThickness = 1,
                    Fill = new SolidColorBrush(Color.Parse("#1A7AF0B0")),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(r, s.X);
                Canvas.SetTop(r, s.Y);
                canvas.Children.Add(r);
                _overlays.Add(r);
            }
        }

        // ---------- ponteiro (arrastar / mover / auto / pan) ----------

        private void OnCanvasPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_texturePath == null)
                return;
            var canvas = this.FindControl<Canvas>("PickCanvas")!;
            var props = e.GetCurrentPoint(canvas).Properties;

            if (props.IsMiddleButtonPressed)
            {
                _panning = true;
                _panStart = e.GetPosition(this);
                _panOffset = this.FindControl<ScrollViewer>("Scroll")!.Offset;
                e.Pointer.Capture(canvas);
                return;
            }

            _start = e.GetPosition(canvas);
            var rect = this.FindControl<Rectangle>("SelectionRect")!;

            if (rect.IsVisible && InSelection(_start))
            {
                _movingSel = true;
                _moveGrab = new Point(_start.X - _selection.X, _start.Y - _selection.Y);
            }
            else
            {
                _creating = true;
                Canvas.SetLeft(rect, _start.X);
                Canvas.SetTop(rect, _start.Y);
                rect.Width = 0; rect.Height = 0; rect.IsVisible = true;
            }
            e.Pointer.Capture(canvas);
        }

        private void OnCanvasMoved(object? sender, PointerEventArgs e)
        {
            if (_panning)
            {
                var d = e.GetPosition(this) - _panStart;
                this.FindControl<ScrollViewer>("Scroll")!.Offset = _panOffset - d;
                return;
            }

            var canvas = this.FindControl<Canvas>("PickCanvas")!;
            var rect = this.FindControl<Rectangle>("SelectionRect")!;
            var p = e.GetPosition(canvas);

            if (_movingSel)
            {
                double x = Clamp(p.X - _moveGrab.X, 0, canvas.Width - _selection.Width);
                double y = Clamp(p.Y - _moveGrab.Y, 0, canvas.Height - _selection.Height);
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
            }
            else if (_creating)
            {
                Canvas.SetLeft(rect, Math.Min(p.X, _start.X));
                Canvas.SetTop(rect, Math.Min(p.Y, _start.Y));
                rect.Width = Math.Abs(p.X - _start.X);
                rect.Height = Math.Abs(p.Y - _start.Y);
            }
        }

        private void OnCanvasReleased(object? sender, PointerReleasedEventArgs e)
        {
            e.Pointer.Capture(null);
            var canvas = this.FindControl<Canvas>("PickCanvas")!;
            var rect = this.FindControl<Rectangle>("SelectionRect")!;

            if (_panning) { _panning = false; return; }

            if (_creating)
            {
                _creating = false;
                if (rect.Width < 3 && rect.Height < 3)
                {
                    // clique simples: auto-dimensiona pelo sprite sob o cursor.
                    var frame = _frames.FirstOrDefault(f => f.Contains((int)_start.X, (int)_start.Y));
                    if (frame.Width > 0) { SetSelection(frame); return; }
                    rect.IsVisible = false; AddEnabled(false); _selection = XnaRectangle.Empty;
                    return;
                }
            }
            _movingSel = false;

            // Finaliza o retângulo (do desenho ou do move).
            int x = (int)Math.Round(Canvas.GetLeft(rect));
            int y = (int)Math.Round(Canvas.GetTop(rect));
            int w = (int)Math.Round(rect.Width);
            int h = (int)Math.Round(rect.Height);
            x = Clamp(x, 0, (int)canvas.Width);
            y = Clamp(y, 0, (int)canvas.Height);
            w = Clamp(w, 0, (int)canvas.Width - x);
            h = Clamp(h, 0, (int)canvas.Height - y);
            if (w < 2 || h < 2) { rect.IsVisible = false; AddEnabled(false); return; }
            SetSelection(new XnaRectangle(x, y, w, h));
        }

        private void SetSelection(XnaRectangle r)
        {
            _selection = r;
            var rect = this.FindControl<Rectangle>("SelectionRect")!;
            Canvas.SetLeft(rect, r.X); Canvas.SetTop(rect, r.Y);
            rect.Width = r.Width; rect.Height = r.Height; rect.IsVisible = true;
            AddEnabled(true);
            this.FindControl<TextBlock>("Info")!.Text = $"Recorte: {r.X}, {r.Y} · {r.Width}×{r.Height}px";
        }

        private bool InSelection(Point p)
            => p.X >= _selection.X && p.X <= _selection.X + _selection.Width
            && p.Y >= _selection.Y && p.Y <= _selection.Y + _selection.Height;

        private void OnCanvasWheel(object? sender, PointerWheelEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                SetZoom(_zoom * (e.Delta.Y > 0 ? 1.15 : 1 / 1.15));
                e.Handled = true;
            }
        }

        // ---------- zoom ----------

        private void OnZoomIn(object? sender, RoutedEventArgs e) => SetZoom(_zoom * 1.25);
        private void OnZoomOut(object? sender, RoutedEventArgs e) => SetZoom(_zoom / 1.25);
        private void OnZoomReset(object? sender, RoutedEventArgs e) => SetZoom(1);

        private void SetZoom(double z)
        {
            _zoom = Math.Clamp(z, 0.1, 8);
            var zoomer = this.FindControl<LayoutTransformControl>("Zoomer")!;
            zoomer.LayoutTransform = new ScaleTransform(_zoom, _zoom);
            this.FindControl<TextBlock>("ZoomLabel")!.Text = $"{_zoom * 100:0}%";
        }

        // ---------- criar/nomear/detectar ----------

        private void OnAddSelection(object? sender, RoutedEventArgs e)
        {
            if (_texturePath == null || _selection.Width <= 0)
                return;
            _library.Stamps.Add(new AtlasStamp
            {
                Name = _library.NextName(), X = _selection.X, Y = _selection.Y, W = _selection.Width, H = _selection.Height
            });
            _library.Save(_texturePath);
            RebuildStamps();
            var list = this.FindControl<ListBox>("StampList")!;
            list.SelectedIndex = _stamps.Count - 1;
        }

        private void OnDetectAll(object? sender, RoutedEventArgs e)
        {
            if (_texturePath == null || _frames.Count == 0)
                return;
            foreach (var f in _frames)
                _library.Stamps.Add(new AtlasStamp { Name = _library.NextName(), X = f.X, Y = f.Y, W = f.Width, H = f.Height });
            _library.Save(_texturePath);
            RebuildStamps();
            this.FindControl<TextBlock>("Info")!.Text = $"{_frames.Count} carimbos criados do atlas.";
        }

        private void OnRenameCommit(object? sender, RoutedEventArgs e)
        {
            if (_texturePath != null)
                _library.Save(_texturePath); // o nome já foi atualizado no modelo pelo binding
        }

        private void OnDeleteSelected(object? sender, RoutedEventArgs e)
        {
            if (Selected() is not { } vm || _texturePath == null)
                return;
            _library.Stamps.Remove(vm.Model);
            _library.Save(_texturePath);
            RebuildStamps();
        }

        // ---------- selecionar / usar ----------

        private void OnStampSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (Selected() is { } vm)
                SetSelection(new XnaRectangle(vm.Model.X, vm.Model.Y, vm.Model.W, vm.Model.H));
        }

        private void OnUseSelected(object? sender, RoutedEventArgs e)
        {
            if (_texturePath == null || _selection.Width <= 0)
                return;
            _editor.SetCurrentStamp(_texturePath, _selection);
            this.FindControl<TextBlock>("Info")!.Text = "Modo carimbo ligado — clique na cena para posicionar (Esc sai).";
        }

        private StampVm? Selected() => this.FindControl<ListBox>("StampList")!.SelectedItem as StampVm;

        private void AddEnabled(bool on) => this.FindControl<Button>("AddButton")!.IsEnabled = on;

        private static double Clamp(double v, double min, double max) => v < min ? min : v > max ? max : v;
        private static int Clamp(int v, int min, int max) => v < min ? min : v > max ? max : v;
    }

    /// <summary>Linha da lista de carimbos (nome editável).</summary>
    public sealed class StampVm : NotificationObject
    {
        public AtlasStamp Model { get; }
        public StampVm(AtlasStamp model) => Model = model;

        public string Name
        {
            get => Model.Name;
            set { Model.Name = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string SizeText => $"{Model.W}×{Model.H}";
    }
}
