using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using DreamBit.Engine.Diagnostics;
using DreamBit.Engine.Serialization;
using DreamBit.Studio.ViewModels;
using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

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

            KeyDown += OnKeyDown;
            _editor.ToolChanged += RebuildPalette;
            _editor.SelectionChanged += RebuildPalette;
            _editor.SelectionChanged += SyncHierarchySelection;

            // Rola o console para o fim quando chega log novo (após o layout medir a linha).
            _editor.Log.Entries.CollectionChanged += (_, _) =>
                Dispatcher.UIThread.Post(() =>
                {
                    var scroll = this.FindControl<ScrollViewer>("ConsoleScroll");
                    if (scroll != null)
                        scroll.Offset = scroll.Offset.WithY(scroll.Extent.Height);
                }, DispatcherPriority.Background);
        }

        private void InvalidateScene() => this.FindControl<SceneView>("Scene")?.InvalidateVisual();

        private void OnTick(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            var dt = now - _lastTick;
            _lastTick = now;

            if (_editor.IsPlaying)
                _editor.Scene.Update(new XnaGameTime(TimeSpan.Zero, dt));

            InvalidateScene();
        }

        private void OnPlayToggle(object? sender, RoutedEventArgs e)
        {
            _editor.IsPlaying = !_editor.IsPlaying;
            if (this.FindControl<Button>("PlayButton")?.Content is PathIcon icon)
                icon.Data = (global::Avalonia.Media.Geometry)global::Avalonia.Application.Current!
                    .FindResource(_editor.IsPlaying ? "IconPause" : "IconPlay")!;
        }

        private void OnPreferences(object? sender, RoutedEventArgs e)
            => new PreferencesWindow().ShowDialog(this);

        // Undo de transform digitado no inspetor: captura ao focar, registra ao sair (se mudou).
        private EditorViewModel.TransformState? _inspectorBefore;

        private void OnTransformFocus(object? sender, GotFocusEventArgs e)
        {
            if (_editor.SelectedObject != null)
                _inspectorBefore = EditorViewModel.Capture(_editor.SelectedObject);
        }

        private void OnTransformBlur(object? sender, RoutedEventArgs e)
        {
            var obj = _editor.SelectedObject;
            if (_inspectorBefore == null || obj == null)
                return;

            var after = EditorViewModel.Capture(obj);
            if (!after.Equals(_inspectorBefore.Value))
                _editor.PushGroupTransform(new[] { obj }, new[] { _inspectorBefore.Value }, new[] { after });
            _inspectorBefore = null;
        }

        private void OnDuplicate(object? sender, RoutedEventArgs e) { _editor.DuplicateSelected(); InvalidateScene(); }
        private void OnCopy(object? sender, RoutedEventArgs e) => _editor.CopySelected();
        private void OnPaste(object? sender, RoutedEventArgs e) { _editor.Paste(); InvalidateScene(); }
        private void OnZoom100(object? sender, RoutedEventArgs e) => _editor.Camera.Zoom = 1f;
        private void OnFocusSelection(object? sender, RoutedEventArgs e) => _editor.Camera.Position = _editor.SelectionCenter();

        // ---- projeto / assets / cena ----

        private async void OnOpenProject(object? sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Abrir pasta do projeto",
                AllowMultiple = false
            });

            var path = folders.FirstOrDefault()?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
                _editor.OpenProjectFolder(path);
        }

        private async void OnSaveScene(object? sender, RoutedEventArgs e)
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Salvar cena",
                DefaultExtension = "dbscene",
                SuggestedFileName = _editor.Scene.Name,
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Cena DreamBit") { Patterns = new[] { "*.dbscene" } }
                }
            });

            var path = file?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
                _editor.SaveTo(path);
        }

        private AtlasPicker? _atlasPicker;

        private void OnOpenAtlas(object? sender, RoutedEventArgs e)
        {
            if (_atlasPicker == null)
            {
                _atlasPicker = new AtlasPicker(_editor);
                _atlasPicker.Closed += (_, _) => _atlasPicker = null;
                _atlasPicker.Show(this);
            }
            else
            {
                _atlasPicker.Activate();
            }
        }

        private void OnExitStamp(object? sender, RoutedEventArgs e) => _editor.ClearStamp();

        // ---- cenas / abas ----

        private void OnNewScene(object? sender, RoutedEventArgs e) => _editor.NewScene();

        private async void OnOpenScene(object? sender, RoutedEventArgs e)
        {
            var path = await PickOpenFileAsync("Abrir cena", "Cena DreamBit", "*.dbscene");
            if (path != null)
                _editor.LoadFrom(path);
        }

        private void OnSceneActivated(object? sender, RoutedEventArgs e)
        {
            if (this.FindControl<ListBox>("ScenesList")?.SelectedItem is string sceneFile)
                _editor.OpenScene(sceneFile);
        }

        private void OnActivateTab(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: SceneTab tab })
                _editor.ActivateTab(tab);
        }

        private void OnCloseTab(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: SceneTab tab })
                _editor.CloseTab(tab);
        }

        // ---- prefabs ----

        private async void OnSavePrefab(object? sender, RoutedEventArgs e)
        {
            if (_editor.SelectedObject == null)
            {
                EngineLog.Warn("Selecione um objeto para salvar como prefab.");
                return;
            }
            var path = await PickSaveFileAsync("Salvar prefab", _editor.SelectedObject.Name, "dbprefab", "Prefab DreamBit");
            if (path != null)
                _editor.SaveSelectedAsPrefab(path);
        }

        private async void OnInsertPrefab(object? sender, RoutedEventArgs e)
        {
            var path = await PickOpenFileAsync("Inserir prefab", "Prefab DreamBit", "*.dbprefab");
            if (path != null)
                _editor.InsertPrefab(path);
        }

        // ---- tilemap ----

        private async void OnImportTmx(object? sender, RoutedEventArgs e)
        {
            var path = await PickOpenFileAsync("Importar mapa Tiled", "Mapa Tiled", "*.tmx");
            if (path != null)
                _editor.ImportTilemap(path);
        }

        private async void OnNewTilemap(object? sender, RoutedEventArgs e)
        {
            var path = await PickOpenFileAsync("Tileset (PNG)", "Imagem PNG", "*.png");
            if (path == null)
                return;

            const int tile = 16;
            var (w, h) = DreamBit.Studio.ImageInfo.GetPngSize(path);
            if (w <= 0 || h <= 0)
            {
                EngineLog.Warn("Não foi possível ler o tamanho do PNG.");
                return;
            }
            _editor.CreateTilemap(path, tile, tile, w / tile, h / tile);
        }

        // ---- seletores de arquivo do inspetor ----

        private async void OnPickTexture(object? sender, RoutedEventArgs e)
        {
            var path = await PickOpenFileAsync("Escolher textura", "Imagens", "*.png");
            if (path != null) _editor.Inspector.SpriteTexturePath = path;
        }

        private async void OnPickAnimTexture(object? sender, RoutedEventArgs e)
        {
            var path = await PickOpenFileAsync("Escolher sprite sheet", "Imagens", "*.png");
            if (path != null) _editor.Inspector.AnimTexturePath = path;
        }

        private async void OnPickSound(object? sender, RoutedEventArgs e)
        {
            var path = await PickOpenFileAsync("Escolher som", "Som WAV", "*.wav");
            if (path != null) _editor.Inspector.AudioPath = path;
        }

        // ---- multisseleção na hierarquia ----

        private bool _syncingHierarchy;

        private void OnHierarchySelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_syncingHierarchy)
                return;

            var list = this.FindControl<ListBox>("HierarchyList");
            if (list == null)
                return;

            var selected = list.SelectedItems?.Cast<DreamBit.Engine.Elements.GameObject>().ToList()
                           ?? new List<DreamBit.Engine.Elements.GameObject>();
            _editor.SetSelection(selected);
        }

        private void SyncHierarchySelection()
        {
            var list = this.FindControl<ListBox>("HierarchyList");
            if (list?.SelectedItems == null)
                return;

            _syncingHierarchy = true;
            list.SelectedItems.Clear();
            foreach (var obj in _editor.SelectedObjects)
                list.SelectedItems.Add(obj);
            _syncingHierarchy = false;
        }

        // ---- paleta de tilemap ----

        private DreamBit.Engine.Tilemap.Tileset? _paletteTileset;

        private void RebuildPalette()
        {
            var image = this.FindControl<Image>("PaletteImage");
            var canvas = this.FindControl<Canvas>("PaletteCanvas");
            var sel = this.FindControl<Rectangle>("PaletteSelection");
            if (image == null || canvas == null || sel == null)
                return;

            _paletteTileset = null;
            image.Source = null;
            sel.IsVisible = false;

            var map = _editor.ActiveTilemap?.Map;
            if (!_editor.IsTilemapTool || map == null)
                return;

            var tileset = map.Tilesets.FirstOrDefault(t => !string.IsNullOrEmpty(t.ResolvedImagePath));
            if (tileset == null)
                return;

            var bmp = AvaloniaImageCache.Get(tileset.ResolvedImagePath);
            if (bmp == null)
                return;

            _paletteTileset = tileset;
            image.Source = bmp;
            canvas.Width = bmp.PixelSize.Width;
            canvas.Height = bmp.PixelSize.Height;
        }

        private void OnPalettePick(object? sender, PointerPressedEventArgs e)
        {
            var canvas = this.FindControl<Canvas>("PaletteCanvas");
            var sel = this.FindControl<Rectangle>("PaletteSelection");
            if (_paletteTileset is not { Columns: > 0 } ts || canvas == null || sel == null)
                return;

            var p = e.GetPosition(canvas);
            int col = (int)(p.X / ts.TileWidth);
            int row = (int)(p.Y / ts.TileHeight);
            if (col < 0 || row < 0 || col >= ts.Columns)
                return;

            _editor.BrushGid = ts.FirstGid + row * ts.Columns + col;

            Canvas.SetLeft(sel, col * ts.TileWidth);
            Canvas.SetTop(sel, row * ts.TileHeight);
            sel.Width = ts.TileWidth;
            sel.Height = ts.TileHeight;
            sel.IsVisible = true;
        }

        // ---- rodar / exportar / conteúdo ----

        private void OnRunGame(object? sender, RoutedEventArgs e)
        {
            var scenePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dreambit_play.dbscene");
            SceneSerializer.Save(_editor.Scene, scenePath);
            try
            {
                DreamBit.Studio.PlayerLauncher.Launch(scenePath);
                EngineLog.Info("Iniciando o DreamBit.Player…");
            }
            catch (Exception ex)
            {
                EngineLog.Error("Não foi possível iniciar o Player: " + ex.Message);
            }
        }

        private async void OnExportGame(object? sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { Title = "Pasta de destino do jogo", AllowMultiple = false });
            var dir = folders.FirstOrDefault()?.TryGetLocalPath();
            if (string.IsNullOrEmpty(dir))
                return;

            string sceneJson = SceneSerializer.SaveToString(_editor.Scene);
            EngineLog.Info("Exportando o jogo… (pode demorar — publica o Player)");
            var (ok, message) = await Task.Run(() => DreamBit.Studio.GameExporter.Publish(dir, sceneJson));
            if (ok) EngineLog.Info(message); else EngineLog.Error(message);
        }

        private void OnBuildContent(object? sender, RoutedEventArgs e)
        {
            var project = _editor.Project.Project;
            if (project == null)
            {
                EngineLog.Warn("Abra um projeto primeiro (botão Projeto).");
                return;
            }
            var (ok, message) = DreamBit.Studio.ContentBuilder.Build(project);
            if (ok) EngineLog.Info(message); else EngineLog.Error(message);
        }

        private void OnToggleTheme(object? sender, RoutedEventArgs e)
        {
            var app = global::Avalonia.Application.Current!;
            app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light : ThemeVariant.Dark;
        }

        // ---- atalhos de teclado ----

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool inText = FocusManager?.GetFocusedElement() is TextBox;

            if (_editor.IsLedgeTool && e.Key == Key.Escape) { _editor.CancelLedge(); InvalidateScene(); }
            else if (_editor.IsLedgeTool && (e.Key == Key.Enter || e.Key == Key.Return)) { _editor.FinishLedge(); InvalidateScene(); }
            else if (e.Key == Key.Delete && !inText)
            {
                if (_editor.DeleteObjectCommand.CanExecute(null)) _editor.DeleteObjectCommand.Execute(null);
                else _editor.DeleteLastLedge();
            }
            else if (ctrl && e.Key == Key.S) OnSaveScene(this, e);
            else if (ctrl && e.Key == Key.Z && _editor.UndoCommand.CanExecute(null)) _editor.UndoCommand.Execute(null);
            else if (ctrl && e.Key == Key.Y && _editor.RedoCommand.CanExecute(null)) _editor.RedoCommand.Execute(null);
            else if (ctrl && e.Key == Key.D) _editor.DuplicateSelected();
            else if (ctrl && e.Key == Key.C && !inText) _editor.CopySelected();
            else if (ctrl && e.Key == Key.V && !inText) _editor.Paste();
            else if (ctrl && (e.Key == Key.D0 || e.Key == Key.NumPad0)) _editor.Camera.Zoom = 1f;
            else if (e.Key == Key.F && !inText) _editor.Camera.Position = _editor.SelectionCenter();
            else if (!inText && TryNudge(e.Key)) { }
            else return;

            e.Handled = true;
            InvalidateScene();
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
            if (delta == XnaVector2.Zero) return false;
            _editor.Nudge(delta);
            return true;
        }

        // ---- diálogos de arquivo ----

        private async Task<string?> PickOpenFileAsync(string title, string typeName, string pattern)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType(typeName) { Patterns = new[] { pattern } } }
            });
            return files.FirstOrDefault()?.TryGetLocalPath();
        }

        private async Task<string?> PickSaveFileAsync(string title, string suggested, string ext, string typeName)
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                DefaultExtension = ext,
                SuggestedFileName = suggested,
                FileTypeChoices = new[] { new FilePickerFileType(typeName) { Patterns = new[] { "*." + ext } } }
            });
            return file?.TryGetLocalPath();
        }

        private void OnAssetActivated(object? sender, RoutedEventArgs e)
        {
            if (this.FindControl<ListBox>("AssetsList")?.SelectedItem is string asset)
            {
                _editor.UseAsset(asset);
                _editor.Inspector.Refresh();
                InvalidateScene();
            }
        }

        // ---- adicionar componentes ----

        private void OnAddSprite(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddSprite);
        private void OnAddAnimator(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddAnimator);
        private void OnAddPlatformer(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddPlatformer);
        private void OnAddCollider(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddCollider);
        private void OnAddAudio(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddAudio);
        private void OnAddParticles(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddParticles);
        private void OnAddTrigger(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddTrigger);
        private void OnAddMessageListener(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddMessageListener);
        private void OnAddFollow(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddFollow);
        private void OnAddRotator(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddRotator);
        private void OnAddBone(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddBone);
        private void OnAddSkeleton(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddSkeleton);
        private void OnAddScript(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddScript);

        // ---- remover componentes ----

        private void OnRemoveSprite(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveSprite);
        private void OnRemoveAnimator(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAnimator);
        private void OnRemovePlatformer(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemovePlatformer);
        private void OnRemoveCollider(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveCollider);
        private void OnRemoveAudio(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAudio);
        private void OnRemoveParticles(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveParticles);
        private void OnRemoveTrigger(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveTrigger);
        private void OnRemoveMessageListener(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveMessageListener);
        private void OnRemoveFollow(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveFollow);
        private void OnRemoveRotator(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveRotator);
        private void OnRemoveBone(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveBone);
        private void OnRemoveSkeleton(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveSkeleton);
        private void OnRemoveScript(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveScript);

        private void OnCompileScript(object? sender, RoutedEventArgs e) => _editor.Inspector.CompileScript();
        private void OnCaptureBonePose(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.CaptureBonePose);
        private void OnResetBonePose(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.ResetBonePose);

        private void OnAddSkeletonKeyframe(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.AddSkeletonKeyframe);
        private void OnRemoveSkeletonKeyframe(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.RemoveSkeletonKeyframe);

        private void OnGoToKeyframe(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: float time })
            {
                _editor.Inspector.GoToSkeletonTime(time);
                InvalidateScene();
            }
        }

        /// <summary>Executa a operação de componente e atualiza inspetor + cena.</summary>
        private void AddComponent(Action operation)
        {
            operation();
            _editor.Inspector.Refresh();
            InvalidateScene();
        }
    }
}
