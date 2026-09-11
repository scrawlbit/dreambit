using System.Linq;
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

            _editor.SelectionChanged += SyncHierarchySelection;

            Surface.LoadContent += (_, device) =>
            {
                _renderer.Initialize(device);
                ThemeManager.Apply(ThemeManager.IsDark, _renderer); // sincroniza cores do canvas
            };
            Surface.Draw += (_, e) =>
            {
                if (_editor.IsPlaying)
                    _editor.Scene.Update(new Microsoft.Xna.Framework.GameTime(
                        System.TimeSpan.Zero, System.TimeSpan.FromSeconds(e.DeltaSeconds)));

                _renderer.HighlightedLedge = _editor.SelectedLedge;
                _renderer.SelectionBox = _input.BoxSelectWorld;
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
                bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                _input.PrimaryDown(Pos(e), W, H, ctrl);
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
            else if (e.Key == Key.Delete)
            {
                if (_editor.DeleteObjectCommand.CanExecute(null))
                    _editor.DeleteObjectCommand.Execute(null);
                else
                    _editor.DeleteLastLedge(); // sem seleção: remove a última ledge
            }
            else if (ctrl && e.Key == Key.Z && _editor.UndoCommand.CanExecute(null))
                _editor.UndoCommand.Execute(null);
            else if (ctrl && e.Key == Key.Y && _editor.RedoCommand.CanExecute(null))
                _editor.RedoCommand.Execute(null);
            else if (ctrl && e.Key == Key.D)
                _editor.DuplicateSelected();
            else if (ctrl && (e.Key == Key.D0 || e.Key == Key.NumPad0))
                _editor.Camera.Zoom = 1f; // zoom 100%
            else if (!(e.OriginalSource is System.Windows.Controls.TextBox) && TryNudge(e.Key))
                e.Handled = true;
        }

        private bool TryNudge(Key key)
        {
            float step = _editor.SnapToGrid ? _editor.GridStep : 1f;
            var delta = key switch
            {
                Key.Left => new XnaVector2(-step, 0),
                Key.Right => new XnaVector2(step, 0),
                Key.Up => new XnaVector2(0, -step),
                Key.Down => new XnaVector2(0, step),
                _ => XnaVector2.Zero
            };

            if (delta == XnaVector2.Zero)
                return false;

            _editor.Nudge(delta);
            return true;
        }

        private void Surface_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_editor.IsLedgeTool)
                _editor.FinishLedge();
        }

        private void OnToggleTheme(object sender, RoutedEventArgs e) => ThemeManager.Toggle(_renderer);

        private void OnActivateTab(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)sender).DataContext is ViewModels.SceneTab tab)
                _editor.ActivateTab(tab);
        }

        private void OnCloseTab(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)sender).DataContext is ViewModels.SceneTab tab)
                _editor.CloseTab(tab);
        }

        private void OnPlayToggle(object sender, RoutedEventArgs e)
        {
            _editor.IsPlaying = !_editor.IsPlaying;
            PlayButton.Content = _editor.IsPlaying ? "⏸ Stop" : "▶ Play";
        }

        private void OnPickTexture(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Todos os arquivos (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
                _editor.Inspector.SpriteTexturePath = dialog.FileName;
        }

        private void OnClearTexture(object sender, RoutedEventArgs e) => _editor.Inspector.SpriteTexturePath = string.Empty;

        private void OnAddSprite(object sender, RoutedEventArgs e) => _editor.AddSprite();
        private void OnRemoveSprite(object sender, RoutedEventArgs e) => _editor.RemoveSprite();
        private void OnAddRotator(object sender, RoutedEventArgs e) => _editor.AddRotator();
        private void OnRemoveRotator(object sender, RoutedEventArgs e) => _editor.RemoveRotator();
        private void OnAddAnimator(object sender, RoutedEventArgs e) => _editor.AddAnimator();
        private void OnRemoveAnimator(object sender, RoutedEventArgs e) => _editor.RemoveAnimator();
        private void OnRemoveTilemap(object sender, RoutedEventArgs e) => _editor.RemoveTilemap();

        private void OnImportTmx(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Mapa Tiled (*.tmx)|*.tmx" };
            if (dialog.ShowDialog() == true)
                _editor.ImportTilemap(dialog.FileName);
        }

        private void OnChangeTmx(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Mapa Tiled (*.tmx)|*.tmx" };
            if (dialog.ShowDialog() == true)
                _editor.Inspector.TilemapPath = dialog.FileName;
        }

        private void OnPickAnimTexture(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Todos os arquivos (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
                _editor.Inspector.AnimTexturePath = dialog.FileName;
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

        private void OnBuildContent(object sender, RoutedEventArgs e)
        {
            var project = _editor.Project.Project;
            if (project == null)
            {
                MessageBox.Show(this, "Abra um projeto primeiro (botão Projeto).", "Conteúdo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var (ok, message) = ContentBuilder.Build(project);
            MessageBox.Show(this, message, "Build de conteúdo (MGCB)", MessageBoxButton.OK,
                ok ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void OnSceneDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ScenesList.SelectedItem is string sceneFileName)
                _editor.OpenScene(sceneFileName);
        }

        private void OnAssetDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssetsList.SelectedItem is string assetPath)
                _editor.UseAsset(assetPath);
        }

        private bool _syncingHierarchy;

        private void OnHierarchySelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_syncingHierarchy)
                return;

            var selected = HierarchyList.SelectedItems.Cast<DreamBit.Engine.Elements.GameObject>().ToList();
            _editor.SetSelection(selected);
        }

        private void SyncHierarchySelection()
        {
            _syncingHierarchy = true;
            HierarchyList.SelectedItems.Clear();
            foreach (var obj in _editor.SelectedObjects)
                HierarchyList.SelectedItems.Add(obj);
            _syncingHierarchy = false;
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
