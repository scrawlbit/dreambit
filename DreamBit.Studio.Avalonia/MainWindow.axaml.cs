using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DreamBit.Studio.ViewModels;
using XnaGameTime = Microsoft.Xna.Framework.GameTime;

namespace DreamBit.Studio.Avalonia
{
    public partial class MainWindow : Window
    {
        private readonly EditorViewModel _editor = new();
        private readonly DispatcherTimer _timer;
        private DateTime _lastTick = DateTime.UtcNow;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _editor;

            var sceneView = this.FindControl<SceneView>("Scene");
            if (sceneView != null)
                sceneView.Editor = _editor;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            var dt = now - _lastTick;
            _lastTick = now;

            if (_editor.IsPlaying)
                _editor.Scene.Update(new XnaGameTime(TimeSpan.Zero, dt));

            this.FindControl<SceneView>("Scene")?.InvalidateVisual();
        }

        private void OnPlayToggle(object? sender, RoutedEventArgs e)
        {
            _editor.IsPlaying = !_editor.IsPlaying;
            var button = this.FindControl<Button>("PlayButton");
            if (button != null)
                button.Content = _editor.IsPlaying ? "⏸ Stop" : "▶ Play";
        }
    }
}
