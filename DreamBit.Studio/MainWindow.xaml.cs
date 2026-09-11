using System.Windows;
using System.Windows.Input;
using DreamBit.Engine.Rendering;
using DreamBit.Studio.Editing;
using DreamBit.Studio.ViewModels;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace DreamBit.Studio
{
    public partial class MainWindow : Window
    {
        private readonly EditorViewModel _editor = new();
        private readonly SceneRenderer _renderer = new();
        private readonly SceneInputController _input;

        public MainWindow()
        {
            InitializeComponent();

            _input = new SceneInputController(_editor);
            DataContext = _editor;

            Surface.LoadContent += (_, device) => _renderer.Initialize(device);
            Surface.Draw += (_, e) =>
            {
                if (_editor.IsPlaying)
                    _editor.Scene.Update(new Microsoft.Xna.Framework.GameTime(
                        System.TimeSpan.Zero, System.TimeSpan.FromSeconds(e.DeltaSeconds)));

                _renderer.HighlightedLedge = _editor.SelectedLedge;
                _renderer.Render(_editor.Scene, _editor.Camera, e.Width, e.Height);
            };
        }

        private XnaVector2 Pos(MouseEventArgs e)
        {
            var p = e.GetPosition(Surface);
            return new XnaVector2((float)p.X, (float)p.Y);
        }

        private int W => (int)Surface.ActualWidth;
        private int H => (int)Surface.ActualHeight;

        private XnaVector2 World(MouseEventArgs e) => _editor.Camera.ScreenToWorld(Pos(e), W, H);

        private void Surface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Surface.Focus();
            Surface.CaptureMouse();

            if (_editor.IsLedgeTool)
            {
                if (e.ChangedButton == MouseButton.Left)
                    _editor.AddLedgePoint(World(e));
                else if (e.ChangedButton == MouseButton.Right)
                    _editor.FinishLedge();
                else if (e.ChangedButton == MouseButton.Middle)
                    _input.MiddleDown(Pos(e));
                return;
            }

            // Ferramenta de seleção
            if (e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Right)
            {
                _input.MiddleDown(Pos(e));
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                _input.PrimaryDown(Pos(e), W, H);
                if (_editor.SelectedObject == null)
                    _editor.TrySelectLedgeAt(World(e), 8f / _editor.Camera.Zoom);
            }
        }

        private void Surface_MouseMove(object sender, MouseEventArgs e) => _input.Move(Pos(e), W, H);

        private void Surface_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _input.Up();
            Surface.ReleaseMouseCapture();
        }

        private void Surface_MouseWheel(object sender, MouseWheelEventArgs e) => _input.Wheel(Pos(e), e.Delta, W, H);

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

            if (_editor.IsLedgeTool && e.Key == Key.Escape)
                _editor.CancelLedge();
            else if (_editor.IsLedgeTool && (e.Key == Key.Enter || e.Key == Key.Return))
                _editor.FinishLedge();
            else if (e.Key == Key.Delete && _editor.DeleteObjectCommand.CanExecute(null))
                _editor.DeleteObjectCommand.Execute(null);
            else if (ctrl && e.Key == Key.Z && _editor.UndoCommand.CanExecute(null))
                _editor.UndoCommand.Execute(null);
            else if (ctrl && (e.Key == Key.Y) && _editor.RedoCommand.CanExecute(null))
                _editor.RedoCommand.Execute(null);
        }

        private void Surface_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_editor.IsLedgeTool)
                _editor.FinishLedge();
        }

        private void OnPlayToggle(object sender, RoutedEventArgs e)
        {
            _editor.IsPlaying = !_editor.IsPlaying;
            PlayButton.Content = _editor.IsPlaying ? "⏸ Stop" : "▶ Play";
        }

        private void OnRunGame(object sender, RoutedEventArgs e)
        {
            var scenePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dreambit_play.dbscene");
            DreamBit.Engine.Serialization.SceneSerializer.Save(_editor.Scene, scenePath);

            try
            {
                DreamBit.Studio.PlayerLauncher.Launch(scenePath);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, "Não foi possível iniciar o DreamBit.Player.\n\n" + ex.Message,
                    "Rodar jogo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private const string SceneFilter = "Cena DreamBit (*.dbscene)|*.dbscene|Todos os arquivos (*.*)|*.*";

        private void OnNewScene(object sender, RoutedEventArgs e) => _editor.NewScene();

        private void OnOpenProject(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Selecione a pasta do projeto" };
            if (dialog.ShowDialog() == true)
                _editor.OpenProjectFolder(dialog.FolderName);
        }

        private void OnSceneDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ScenesList.SelectedItem is string sceneFileName)
                _editor.OpenScene(sceneFileName);
        }

        private void OnOpenScene(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = SceneFilter };
            if (dialog.ShowDialog() == true)
                _editor.LoadFrom(dialog.FileName);
        }

        private void OnSaveScene(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = SceneFilter,
                FileName = _editor.CurrentPath ?? "cena.dbscene"
            };
            if (dialog.ShowDialog() == true)
                _editor.SaveTo(dialog.FileName);
        }
    }
}
