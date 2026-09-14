using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DreamBit.Engine.Tilemap;
using DreamBit.Engine.Diagnostics;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using DreamBit.Studio.ViewModels;
using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace DreamBit.Studio.Avalonia
{
    public partial class MainWindow : Window
    {
        private readonly EditorViewModel _editor = new();
        private DreamBit.Studio.EditorPreferences _keymap = DreamBit.Studio.EditorPreferences.Load();
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

            // Arrastar na hierarquia para reordenar a ordem de exibição.
            var hier = this.FindControl<TreeView>("HierarchyList");
            if (hier != null)
            {
                hier.AddHandler(PointerPressedEvent, OnHierarchyDragStart, RoutingStrategies.Tunnel);
                hier.AddHandler(PointerMovedEvent, OnHierarchyDragMove, RoutingStrategies.Tunnel);
                DragDrop.SetAllowDrop(hier, true);
                hier.AddHandler(DragDrop.DragOverEvent, OnHierarchyDragOver);
                hier.AddHandler(DragDrop.DropEvent, OnHierarchyDrop);
            }

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
            {
                var scene = this.FindControl<SceneView>("Scene");
                if (scene != null)
                    DreamBit.Engine.Input.Input.SetPointer(scene.PointerScreen, scene.PointerIsDown);
                _editor.Scene.Update(new XnaGameTime(TimeSpan.Zero, dt));
            }

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

        private void OnSliceAnimator(object? sender, RoutedEventArgs e) => _editor.Inspector.SliceAnimatorGrid();

        private void OnAutoDetectFrames(object? sender, RoutedEventArgs e)
        {
            var path = _editor.Inspector.AnimTexturePath;
            if (PngMask.TryLoad(path, out var opaque, out int w, out int h))
            {
                var rects = DreamBit.Engine.Rendering.FrameDetector.Detect(opaque, w, h);
                _editor.Inspector.ApplyDetectedFrames(rects);
                InvalidateScene();
            }
        }

        private void OnClearDetectedFrames(object? sender, RoutedEventArgs e)
        {
            _editor.Inspector.ClearDetectedFrames();
            InvalidateScene();
        }

        private void OnDetectSpriteChroma(object? sender, RoutedEventArgs e)
        {
            if (PngMask.TryLoadColors(_editor.Inspector.SpriteTexturePath, out var colors, out int w, out int h))
            {
                var bg = DreamBit.Engine.Rendering.ChromaKey.DetectBackground(colors, w, h);
                _editor.Inspector.SetSpriteChroma(bg);
                InvalidateScene();
            }
        }

        private void OnDetectAnimChroma(object? sender, RoutedEventArgs e)
        {
            if (PngMask.TryLoadColors(_editor.Inspector.AnimTexturePath, out var colors, out int w, out int h))
            {
                var bg = DreamBit.Engine.Rendering.ChromaKey.DetectBackground(colors, w, h);
                _editor.Inspector.SetAnimChroma(bg);
                InvalidateScene();
            }
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

            var list = this.FindControl<TreeView>("HierarchyList");
            if (list?.SelectedItems == null)
                return;

            var selected = list.SelectedItems.Cast<DreamBit.Engine.Elements.GameObject>().ToList();
            _editor.SetSelection(selected);
        }

        private void SyncHierarchySelection()
        {
            var list = this.FindControl<TreeView>("HierarchyList");
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

            RebuildLayers();
        }

        private void OnAddTileLayer(object? sender, RoutedEventArgs e)
        {
            _editor.AddTileLayer();
            RebuildLayers();
            InvalidateScene();
        }

        /// <summary>Reconstrói o painel de camadas do tilemap ativo (topo = frente).</summary>
        private void RebuildLayers()
        {
            var host = this.FindControl<StackPanel>("LayersList");
            if (host == null)
                return;
            host.Children.Clear();

            var layers = _editor.TileLayers;
            // Do topo (última, desenhada por último) para a base.
            for (int i = layers.Count - 1; i >= 0; i--)
                host.Children.Add(LayerRow(layers[i]));
        }

        private Control LayerRow(TileLayer layer)
        {
            bool active = ReferenceEquals(layer, _editor.ActiveLayer);
            var row = new Border
            {
                Background = active ? new SolidColorBrush(Color.FromRgb(0x2E, 0x3A, 0x4A)) : Brushes.Transparent,
                CornerRadius = new global::Avalonia.CornerRadius(3),
                Padding = new global::Avalonia.Thickness(2)
            };
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto,Auto") };

            var vis = new CheckBox { IsChecked = layer.Visible, VerticalAlignment = VerticalAlignment.Center };
            vis.IsCheckedChanged += (_, __) => { _editor.ToggleTileLayerVisible(layer); InvalidateScene(); };
            Grid.SetColumn(vis, 0);

            var name = new Button
            {
                Content = layer.Name,
                Background = Brushes.Transparent,
                Foreground = active ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xB9, 0xC0, 0xCE)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new global::Avalonia.Thickness(6, 2)
            };
            name.Click += (_, __) => { _editor.ActiveLayer = layer; RebuildLayers(); };
            Grid.SetColumn(name, 1);

            var up = SmallButton("▲", () => { _editor.MoveTileLayer(layer, 1); RebuildLayers(); InvalidateScene(); });
            Grid.SetColumn(up, 2);
            var down = SmallButton("▼", () => { _editor.MoveTileLayer(layer, -1); RebuildLayers(); InvalidateScene(); });
            Grid.SetColumn(down, 3);
            var del = SmallButton(CloseGlyph(), () => { _editor.RemoveTileLayer(layer); RebuildLayers(); InvalidateScene(); });
            Grid.SetColumn(del, 4);

            grid.Children.Add(vis);
            grid.Children.Add(name);
            grid.Children.Add(up);
            grid.Children.Add(down);
            grid.Children.Add(del);
            row.Child = grid;
            return row;
        }

        private static Button SmallButton(object content, Action onClick)
        {
            var b = new Button { Content = content, Padding = new global::Avalonia.Thickness(5, 2), Background = Brushes.Transparent, Foreground = new SolidColorBrush(Color.FromRgb(0x8A, 0x93, 0xA6)) };
            b.Click += (_, __) => onClick();
            return b;
        }

        /// <summary>Ícone Material de fechar (para botões criados em código).</summary>
        private global::Avalonia.Controls.PathIcon CloseGlyph()
        {
            var icon = new global::Avalonia.Controls.PathIcon { Width = 11, Height = 11 };
            if (this.FindResource("IconClose") is global::Avalonia.Media.Geometry g)
                icon.Data = g;
            return icon;
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
            bool inText = FocusManager?.GetFocusedElement() is TextBox;

            // Esc sai do modo carimbo.
            if (_editor.StampMode && e.Key == Key.Escape) { _editor.ClearStamp(); e.Handled = true; InvalidateScene(); return; }

            // Alt+Cima/Baixo: reordena o selecionado na hierarquia (muda a ordem de exibição).
            if (!inText && e.KeyModifiers.HasFlag(KeyModifiers.Alt) && (e.Key == Key.Up || e.Key == Key.Down))
            {
                _editor.MoveSelectedInHierarchy(e.Key == Key.Up ? -1 : 1);
                RebuildHierarchy(); e.Handled = true; InvalidateScene(); return;
            }

            // Ferramenta de ledge tem prioridade (Esc/Enter).
            if (_editor.IsLedgeTool && e.Key == Key.Escape) { _editor.CancelLedge(); e.Handled = true; InvalidateScene(); return; }
            if (_editor.IsLedgeTool && (e.Key == Key.Enter || e.Key == Key.Return)) { _editor.FinishLedge(); e.Handled = true; InvalidateScene(); return; }

            // Atalhos configuráveis: casa o gesto pressionado com a ação do keymap.
            var gesture = Gestures.Format(e.KeyModifiers, e.Key);
            var action = _keymap.ActionFor(gesture);
            if (action != null && DispatchShortcut(action, inText))
            {
                e.Handled = true;
                InvalidateScene();
                return;
            }

            if (!inText && TryNudge(e.Key)) { e.Handled = true; InvalidateScene(); }
        }

        /// <summary>Executa a ação de atalho pelo nome. Retorna false se não se aplica agora.</summary>
        private bool DispatchShortcut(string action, bool inText)
        {
            switch (action)
            {
                case "Delete" when !inText:
                    if (_editor.DeleteObjectCommand.CanExecute(null)) _editor.DeleteObjectCommand.Execute(null);
                    else _editor.DeleteLastLedge();
                    return true;
                case "Save": OnSaveScene(this, new RoutedEventArgs()); return true;
                case "Undo" when _editor.UndoCommand.CanExecute(null): _editor.UndoCommand.Execute(null); return true;
                case "Redo" when _editor.RedoCommand.CanExecute(null): _editor.RedoCommand.Execute(null); return true;
                case "Duplicate": _editor.DuplicateSelected(); return true;
                case "Copy" when !inText: _editor.CopySelected(); return true;
                case "Paste" when !inText: _editor.Paste(); return true;
                case "Group" when !inText: _editor.GroupSelected(); RebuildHierarchy(); return true;
                case "Ungroup" when !inText: _editor.UngroupSelected(); RebuildHierarchy(); return true;
                case "Play": OnPlayToggle(this, new RoutedEventArgs()); return true;
                case "RunPlayer": OnRunGame(this, new RoutedEventArgs()); return true;
                case "Atlas": OnOpenAtlas(this, new RoutedEventArgs()); return true;
                case "ZoomReset": _editor.Camera.Zoom = 1f; return true;
                case "FocusSelection" when !inText: _editor.Camera.Position = _editor.SelectionCenter(); return true;
                default: return false;
            }
        }

        /// <summary>Recarrega o keymap das preferências (após editar/importar).</summary>
        public void ReloadKeymap() => _keymap = DreamBit.Studio.EditorPreferences.Load();

        // ---- painéis fecháveis (menu Janela) ----
        private void SetPanelChecked(string menu, bool value)
        {
            var m = this.FindControl<MenuItem>(menu);
            if (m != null) m.IsChecked = value;
        }

        private void SetConsolePanel(bool show)
        {
            var p = this.FindControl<Border>("ConsolePanel");
            if (p != null) p.IsVisible = show;
            SetPanelChecked("MenuConsole", show);
        }

        private void SetInspectorPanel(bool show)
        {
            var p = this.FindControl<DockPanel>("InspectorPanel");
            var split = this.FindControl<GridSplitter>("InspectorSplitter");
            var grid = this.FindControl<Grid>("MainGrid");
            if (p != null) p.IsVisible = show;
            if (split != null) split.IsVisible = show;
            if (grid != null)
            {
                grid.ColumnDefinitions[4].Width = new global::Avalonia.Controls.GridLength(show ? 290 : 0);
                grid.ColumnDefinitions[3].Width = new global::Avalonia.Controls.GridLength(show ? 4 : 0);
            }
            SetPanelChecked("MenuInspector", show);
        }

        private void SetHierarchyPanel(bool show)
        {
            var p = this.FindControl<DockPanel>("HierarchyPanel");
            var split = this.FindControl<GridSplitter>("HierarchySplitter");
            var grid = this.FindControl<Grid>("LeftGrid");
            if (p != null) p.IsVisible = show;
            if (split != null) split.IsVisible = show;
            if (grid != null)
            {
                grid.RowDefinitions[2].Height = show ? global::Avalonia.Controls.GridLength.Star : new global::Avalonia.Controls.GridLength(0);
                grid.RowDefinitions[1].Height = new global::Avalonia.Controls.GridLength(show ? 4 : 0);
            }
            SetPanelChecked("MenuHierarchy", show);
        }

        private void SetAssetsPanel(bool show)
        {
            var p = this.FindControl<DockPanel>("AssetsPanel");
            var split = this.FindControl<GridSplitter>("AssetsSplitter");
            var grid = this.FindControl<Grid>("LeftGrid");
            if (p != null) p.IsVisible = show;
            if (split != null) split.IsVisible = show;
            if (grid != null)
            {
                grid.RowDefinitions[4].Height = new global::Avalonia.Controls.GridLength(show ? 200 : 0);
                grid.RowDefinitions[3].Height = new global::Avalonia.Controls.GridLength(show ? 4 : 0);
            }
            SetPanelChecked("MenuAssets", show);
        }

        private bool PanelVisible(string name) => this.FindControl<Control>(name)?.IsVisible ?? true;

        private void OnToggleConsolePanel(object? sender, RoutedEventArgs e) => SetConsolePanel(!PanelVisible("ConsolePanel"));
        private void OnCloseConsole(object? sender, RoutedEventArgs e) => SetConsolePanel(false);
        private void OnToggleInspectorPanel(object? sender, RoutedEventArgs e) => SetInspectorPanel(!PanelVisible("InspectorPanel"));
        private void OnCloseInspectorPanel(object? sender, RoutedEventArgs e) => SetInspectorPanel(false);
        private void OnToggleHierarchyPanel(object? sender, RoutedEventArgs e) => SetHierarchyPanel(!PanelVisible("HierarchyPanel"));
        private void OnCloseHierarchyPanel(object? sender, RoutedEventArgs e) => SetHierarchyPanel(false);
        private void OnToggleAssetsPanel(object? sender, RoutedEventArgs e) => SetAssetsPanel(!PanelVisible("AssetsPanel"));
        private void OnCloseAssetsPanel(object? sender, RoutedEventArgs e) => SetAssetsPanel(false);

        private void RebuildHierarchy() => SyncHierarchySelection();

        // ---- arrastar na hierarquia (reordenar ordem de exibicao) ----
        private GameObject? _hierDrag;
        private global::Avalonia.Point _hierDragStart;

        private void OnHierarchyDragStart(object? sender, PointerPressedEventArgs e)
        {
            _hierDrag = HierItemUnder(e.Source);
            _hierDragStart = e.GetPosition(this);
        }

        private async void OnHierarchyDragMove(object? sender, PointerEventArgs e)
        {
            if (_hierDrag == null)
                return;
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) { _hierDrag = null; return; }
            var d = e.GetPosition(this) - _hierDragStart;
            if (Math.Abs(d.X) < 5 && Math.Abs(d.Y) < 5)
                return;
            var item = _hierDrag;
            _hierDrag = null;
            var data = new DataObject();
            data.Set("dbobj", item);
            await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
        }

        private void OnHierarchyDragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = e.Data.Contains("dbobj") ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnHierarchyDrop(object? sender, DragEventArgs e)
        {
            if (e.Data.Get("dbobj") is not GameObject dragged)
                return;
            var tvi = (e.Source as global::Avalonia.Visual)?.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
            if (tvi?.DataContext is not GameObject target)
                return;
            bool after = e.GetPosition(tvi).Y > tvi.Bounds.Height / 2;
            _editor.MoveInHierarchy(dragged, target, after);
            RebuildHierarchy();
            InvalidateScene();
            e.Handled = true;
        }

        private static GameObject? HierItemUnder(object? source)
        {
            var tvi = (source as global::Avalonia.Visual)?.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
            return tvi?.DataContext as GameObject;
        }

        private void OnApplyPrefab(object? sender, RoutedEventArgs e) => _editor.ApplyToPrefab();
        private void OnRevertPrefab(object? sender, RoutedEventArgs e) { _editor.RevertToPrefab(); InvalidateScene(); }
        private void OnRevertKeepPrefab(object? sender, RoutedEventArgs e) { _editor.RevertKeepingOverrides(); InvalidateScene(); }

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

        private async void OnAddComponentSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox box || box.SelectedItem is not ComboBoxItem item)
                return;

            var label = item.Content as string;
            box.SelectedItem = null; // volta ao placeholder (re-entra com null e sai)

            if (label == "Script")
            {
                await AddScriptWithFileAsync();
                return;
            }

            System.Action? add = label switch
            {
                "Sprite" => _editor.AddSprite,
                "Animator" => _editor.AddAnimator,
                "Platformer" => _editor.AddPlatformer,
                "Collider" => _editor.AddCollider,
                "Camera" => _editor.AddCamera,
                "Text" => _editor.AddText,
                "Audio" => _editor.AddAudio,
                "Particles" => _editor.AddParticles,
                "Trigger" => _editor.AddTrigger,
                "Message" => _editor.AddMessageListener,
                "Follow" => _editor.AddFollow,
                "Rotator" => _editor.AddRotator,
                "Tween" => _editor.AddTween,
                "Bone" => _editor.AddBone,
                "Skeleton" => _editor.AddSkeleton,
                "Animator Controller" => _editor.AddAnimatorController,
                "UI Anchor" => _editor.AddUiAnchor,
                "UI Button" => _editor.AddUiButton,
                "UI Layout" => _editor.AddUiLayout,
                "UI Slider" => _editor.AddUiSlider,
                "UI Toggle" => _editor.AddUiToggle,
                "UI Progress Bar" => _editor.AddUiProgressBar,
                "UI Text Field" => _editor.AddUiTextField,
                "UI Scroll View" => _editor.AddUiScrollView,
                "UI Navigator" => _editor.AddUiNavigator,
                "Parallax" => _editor.AddParallax,
                "Timer" => _editor.AddTimer,
                "Rigidbody 2D" => _editor.AddRigidbody,
                "Property Animator" => _editor.AddPropertyAnimator,
                "Sprite Animator Controller" => _editor.AddSpriteAnimatorController,
                "Nav Chaser" => _editor.AddNavChaser,
                "Audio Listener" => _editor.AddAudioListener,
                "Light 2D" => _editor.AddLight,
                "Ambient Light" => _editor.AddAmbientLight,
                "Top-Down Controller" => _editor.AddTopDown,
                "Health" => _editor.AddHealth,
                "Hurtbox" => _editor.AddHurtbox,
                "Hitbox" => _editor.AddHitbox,
                "Sprite Flash" => _editor.AddSpriteFlash,
                "Joint 2D" => _editor.AddJoint,
                "Shadow Caster" => _editor.AddShadowCaster,
                "Y Sort" => _editor.AddYSort,
                "State Machine" => _editor.AddStateMachine,
                "Screen Fade" => _editor.AddScreenFade,
                "Post Process" => _editor.AddPostProcess,
                "Layered Music" => _editor.AddLayeredMusic,
                _ => null
            };

            if (add != null)
                AddComponent(add);
        }

        private void OnAddSprite(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddSprite);
        private void OnAddAnimator(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddAnimator);
        private void OnAddPlatformer(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddPlatformer);
        private void OnAddCollider(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddCollider);
        private void OnAddAudio(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddAudio);
        private void OnAddParticles(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddParticles);
        private void OnAddTrigger(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddTrigger);
        private void OnAddMessageListener(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddMessageListener);
        private void OnAddCamera(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddCamera);
        private void OnAddFollow(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddFollow);
        private void OnAddRotator(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddRotator);
        private void OnAddBone(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddBone);
        private void OnAddSkeleton(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddSkeleton);
        private async void OnAddScript(object? sender, RoutedEventArgs e) => await AddScriptWithFileAsync();

        private void OnOpenScriptInIde(object? sender, RoutedEventArgs e)
        {
            var path = _editor.SelectedObject?.Components
                .OfType<DreamBit.Engine.Components.ScriptComponent>().FirstOrDefault()?.SourcePath;
            if (!string.IsNullOrWhiteSpace(path))
                _ = OpenFileInIdeAsync(path!);
        }

        /// <summary>Fluxo de adicionar Script: pede pasta+nome, cria o arquivo .cs real, aponta o
        /// componente pra ele (sem texto no inspetor) e abre na IDE (solução + arquivo).</summary>
        private async System.Threading.Tasks.Task AddScriptWithFileAsync()
        {
            var obj = _editor.SelectedObject;
            if (obj == null || obj.Components.OfType<DreamBit.Engine.Components.ScriptComponent>().Any())
                return;

            var path = await PickSaveFileAsync("Novo script C#", "MeuScript", "cs", "C# script");
            if (string.IsNullOrWhiteSpace(path))
                return;
            if (!path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
                path += ".cs";

            try
            {
                if (!System.IO.File.Exists(path))
                {
                    var className = DreamBit.Studio.IdeLauncher.SanitizeClassName(System.IO.Path.GetFileNameWithoutExtension(path));
                    System.IO.File.WriteAllText(path, DreamBit.Studio.IdeLauncher.ScriptTemplate(className));
                }
            }
            catch
            {
                return; // falhou ao criar o arquivo: não adiciona o componente
            }

            var script = new DreamBit.Engine.Components.ScriptComponent { SourcePath = path };
            _editor.AddScriptComponent(script);
            _editor.Inspector.Refresh();
            InvalidateScene();

            await OpenFileInIdeAsync(path);
        }

        /// <summary>Abre um arquivo na IDE preferida (detecta e guarda a escolha na 1ª vez).</summary>
        private async System.Threading.Tasks.Task OpenFileInIdeAsync(string file)
        {
            var prefs = DreamBit.Studio.EditorPreferences.Load();
            var editor = prefs.ScriptEditorPath;
            if (string.IsNullOrEmpty(editor) || !System.IO.File.Exists(editor))
            {
                editor = DreamBit.Studio.IdeLauncher.AutoDetect() ?? await PickOpenExeAsync();
                if (string.IsNullOrEmpty(editor))
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file) { UseShellExecute = true }); }
                    catch { /* sem programa: silencioso */ }
                    return;
                }
                prefs.ScriptEditorPath = editor;
                prefs.Save();
            }
            DreamBit.Studio.IdeLauncher.Open(editor, file);
        }

        private async System.Threading.Tasks.Task<string?> PickOpenExeAsync()
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Escolha o programa (IDE) para abrir scripts (Rider, Visual Studio, VS Code)",
                AllowMultiple = false
            });
            return files.FirstOrDefault()?.TryGetLocalPath();
        }

        // ---- remover componentes ----

        private void OnRemoveSprite(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveSprite);
        private void OnRemoveAnimator(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAnimator);
        private void OnRemovePlatformer(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemovePlatformer);
        private void OnRemoveCollider(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveCollider);
        private void OnRemoveAudio(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAudio);
        private void OnRemoveParticles(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveParticles);
        private void OnRemoveTrigger(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveTrigger);
        private void OnRemoveMessageListener(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveMessageListener);
        private void OnRemoveCamera(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveCamera);
        private void OnRemoveText(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveText);
        private void OnRemoveTween(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveTween);
        private void OnRemoveAnimatorController(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAnimatorController);
        private void OnRemoveUiAnchor(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiAnchor);
        private void OnRemoveUiButton(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiButton);
        private void OnRemoveUiLayout(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiLayout);
        private void OnRemoveUiSlider(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiSlider);
        private void OnRemoveUiToggle(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiToggle);
        private void OnRemoveUiProgressBar(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiProgressBar);
        private void OnRemoveUiTextField(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiTextField);
        private void OnRemoveUiNavigator(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiNavigator);
        private void OnRemoveUiScrollView(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveUiScrollView);
        private void OnRemoveParallax(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveParallax);
        private void OnRemoveTimer(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveTimer);
        private void OnRemoveRigidbody(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveRigidbody);
        private void OnRemovePropertyAnimator(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemovePropertyAnimator);
        private void OnRemoveSpriteAnimatorController(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveSpriteAnimatorController);
        private void OnRemoveNavChaser(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveNavChaser);
        private void OnRemoveAudioListener(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAudioListener);
        private void OnRemoveLight(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveLight);
        private void OnRemoveAmbientLight(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAmbientLight);
        private void OnRemoveTopDown(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveTopDown);
        private void OnRemoveHealth(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveHealth);
        private void OnRemoveHurtbox(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveHurtbox);
        private void OnRemoveHitbox(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveHitbox);
        private void OnRemoveSpriteFlash(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveSpriteFlash);
        private void OnRemoveJoint(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveJoint);
        private void OnRemoveShadowCaster(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveShadowCaster);
        private void OnRemoveYSort(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveYSort);
        private void OnRemoveStateMachine(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveStateMachine);
        private async void OnEditStateMachine(object? sender, RoutedEventArgs e)
        {
            var fsm = _editor.SelectedObject?.Components.OfType<DreamBit.Engine.Components.AnimationStateMachine>().FirstOrDefault();
            if (fsm == null) return;
            await new StateMachineEditor(fsm).ShowDialog(this);
            _editor.Inspector.Refresh();
        }
        private void OnRemoveScreenFade(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveScreenFade);
        private void OnRemovePostProcess(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemovePostProcess);
        private void OnRemoveLayeredMusic(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveLayeredMusic);
        private void OnAddMusicLayer(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.AddMusicLayer);
        private void OnRemoveMusicLayer(object? sender, RoutedEventArgs e)
        {
            if (sender is Control c && c.DataContext is DreamBit.Studio.ViewModels.MusicLayerRow row)
                AddComponent(() => _editor.Inspector.RemoveMusicLayer(row));
        }
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
        private void OnAddSkeletonClip(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.AddSkeletonClip);
        private void OnRemoveSkeletonClip(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.RemoveSkeletonClip);

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
