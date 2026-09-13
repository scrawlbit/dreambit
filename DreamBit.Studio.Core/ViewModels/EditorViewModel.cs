using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Diagnostics;
using DreamBit.Engine.Editing;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Notification;
using DreamBit.Engine.Project;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using DreamBit.Engine.Tilemap;
using DreamBit.Studio.Mvvm;
using Microsoft.Xna.Framework;

namespace DreamBit.Studio.ViewModels
{
    /// <summary>
    /// Estado do editor: a cena, a câmera, o objeto selecionado, o histórico (undo/redo)
    /// e os comandos da toolbar.
    /// </summary>
    public sealed class EditorViewModel : NotificationObject
    {
        private GameObject? _selectedObject;
        private readonly List<GameObject> _selectedObjects = new();
        private int _counter;
        private bool _isPlaying;
        private Scene _scene;

        private EditorTool _currentTool = EditorTool.Select;
        private Ledge? _selectedLedge;
        private Ledge? _pendingLedge;
        private bool _newLedgeOneWay = true;
        private SceneTab? _activeTab;

        public EditorViewModel()
        {
            _scene = new Scene { Name = "Cena de Exemplo" };
            Camera = new Camera2D();
            Inspector = new InspectorViewModel();
            History = new History();
            Project = new ProjectViewModel();
            Log = new LogViewModel();

            AddObjectCommand = new RelayCommand(() => AddObject());
            DeleteObjectCommand = new RelayCommand(DeleteSelected, () => SelectedObject != null || SelectedLedge != null);
            AddRotatorCommand = new RelayCommand(AddRotator, () => SelectedObject != null);
            UndoCommand = new RelayCommand(History.Undo, () => History.CanUndo);
            RedoCommand = new RelayCommand(History.Redo, () => History.CanRedo);
            SelectToolCommand = new RelayCommand(() => CurrentTool = EditorTool.Select);
            LedgeToolCommand = new RelayCommand(() => CurrentTool = EditorTool.Ledge);
            TilemapToolCommand = new RelayCommand(() => CurrentTool = EditorTool.Tilemap);

            History.Changed += () =>
            {
                UndoCommand.RaiseCanExecuteChanged();
                RedoCommand.RaiseCanExecuteChanged();
                Inspector.Refresh();
            };

            SeedSampleScene();
            History.Clear(); // a cena inicial não entra no histórico

            var initialTab = new SceneTab(_scene) { IsActive = true };
            Tabs.Add(initialTab);
            _activeTab = initialTab;
        }

        public ObservableCollection<SceneTab> Tabs { get; } = new();

        public SceneTab? ActiveTab
        {
            get => _activeTab;
            set => ActivateTab(value);
        }

        public void ActivateTab(SceneTab? tab)
        {
            if (tab == null || _activeTab == tab)
                return;

            SelectedObject = null;
            SelectedLedge = null;
            CancelLedge();

            if (_activeTab != null)
                _activeTab.IsActive = false;

            _activeTab = tab;
            _activeTab.IsActive = true;

            Scene = tab.Scene;
            CurrentPath = tab.Path;
            History.Clear();

            OnPropertyChanged(nameof(ActiveTab));
        }

        public SceneTab AddSceneTab(Scene scene, string? path)
        {
            var tab = new SceneTab(scene, path);
            Tabs.Add(tab);
            ActivateTab(tab);
            return tab;
        }

        public void CloseTab(SceneTab tab)
        {
            int index = Tabs.IndexOf(tab);
            if (index < 0)
                return;

            bool wasActive = tab.IsActive;
            Tabs.Remove(tab);

            if (Tabs.Count == 0)
            {
                NewScene();
                return;
            }

            if (wasActive)
                ActivateTab(Tabs[System.Math.Min(index, Tabs.Count - 1)]);
        }

        public Scene Scene
        {
            get => _scene;
            private set => Set(ref _scene, value);
        }
        public Camera2D Camera { get; }
        public InspectorViewModel Inspector { get; }
        public History History { get; }
        public ProjectViewModel Project { get; }
        public LogViewModel Log { get; }

        public RelayCommand AddObjectCommand { get; }
        public RelayCommand DeleteObjectCommand { get; }
        public RelayCommand AddRotatorCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }
        public RelayCommand SelectToolCommand { get; }
        public RelayCommand LedgeToolCommand { get; }
        public RelayCommand TilemapToolCommand { get; }

        /// <summary>Caminho do arquivo da cena atual, se salva/aberta em disco.</summary>
        public string? CurrentPath { get; private set; }

        /// <summary>Objetos atualmente selecionados (multisseleção).</summary>
        public IReadOnlyList<GameObject> SelectedObjects => _selectedObjects;

        /// <summary>Disparado quando a seleção de objetos muda (para sincronizar a hierarquia).</summary>
        public event System.Action? SelectionChanged;

        /// <summary>Disparado quando a ferramenta ativa muda (para atualizar a paleta).</summary>
        public event System.Action? ToolChanged;

        /// <summary>Objeto principal da seleção (o do inspetor). Setar seleciona só ele.</summary>
        public GameObject? SelectedObject
        {
            get => _selectedObject;
            set => SelectSingle(value);
        }

        /// <summary>Seleciona apenas o objeto informado (ou limpa a seleção).</summary>
        public void SelectSingle(GameObject? obj)
        {
            ClearSelectionFlags();
            _selectedObjects.Clear();
            if (obj != null)
            {
                _selectedObjects.Add(obj);
                obj.IsSelected = true;
            }
            UpdateSelection(obj);
        }

        /// <summary>Adiciona/remove um objeto da seleção (Ctrl+clique).</summary>
        public void ToggleSelect(GameObject obj)
        {
            if (_selectedObjects.Remove(obj))
            {
                obj.IsSelected = false;
                UpdateSelection(_selectedObjects.Count > 0 ? _selectedObjects[^1] : null);
            }
            else
            {
                _selectedObjects.Add(obj);
                obj.IsSelected = true;
                UpdateSelection(obj);
            }
        }

        /// <summary>Substitui a seleção por uma lista (hierarquia / seleção por caixa).</summary>
        public void SetSelection(IEnumerable<GameObject> objects)
        {
            ClearSelectionFlags();
            _selectedObjects.Clear();
            foreach (var obj in objects)
            {
                if (!_selectedObjects.Contains(obj))
                {
                    _selectedObjects.Add(obj);
                    obj.IsSelected = true;
                }
            }
            UpdateSelection(_selectedObjects.Count > 0 ? _selectedObjects[^1] : null);
        }

        private void ClearSelectionFlags()
        {
            foreach (var obj in _selectedObjects)
                obj.IsSelected = false;
        }

        private void UpdateSelection(GameObject? primary)
        {
            _selectedObject = primary;

            if (primary != null && _selectedLedge != null)
            {
                _selectedLedge = null;
                OnPropertyChanged(nameof(SelectedLedge));
                OnPropertyChanged(nameof(HasSelectedLedge));
            }

            Inspector.Target = _selectedObject;
            OnPropertyChanged(nameof(SelectedObject));
            OnPropertyChanged(nameof(SelectedObjects));
            DeleteObjectCommand.RaiseCanExecuteChanged();
            AddRotatorCommand.RaiseCanExecuteChanged();
            SelectionChanged?.Invoke();
        }

        public EditorTool CurrentTool
        {
            get => _currentTool;
            set
            {
                if (Set(ref _currentTool, value))
                {
                    if (value != EditorTool.Ledge)
                        CancelLedge();
                    OnPropertyChanged(nameof(IsSelectTool));
                    OnPropertyChanged(nameof(IsLedgeTool));
                    OnPropertyChanged(nameof(IsTilemapTool));
                    ToolChanged?.Invoke();
                }
            }
        }
        public bool IsSelectTool => _currentTool == EditorTool.Select;
        public bool IsLedgeTool => _currentTool == EditorTool.Ledge;
        public bool IsTilemapTool => _currentTool == EditorTool.Tilemap;

        /// <summary>One-way aplicado às novas ledges desenhadas.</summary>
        public bool NewLedgeOneWay
        {
            get => _newLedgeOneWay;
            set => Set(ref _newLedgeOneWay, value);
        }

        public Ledge? SelectedLedge
        {
            get => _selectedLedge;
            set
            {
                if (_selectedLedge == value)
                    return;

                _selectedLedge = value;

                if (_selectedLedge != null && SelectedObject != null)
                    SelectedObject = null;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedLedge));
                DeleteObjectCommand.RaiseCanExecuteChanged();
            }
        }
        public bool HasSelectedLedge => _selectedLedge != null;

        private string? _playSnapshot;

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (!Set(ref _isPlaying, value))
                    return;

                if (value)
                {
                    _playSnapshot = SceneSerializer.SaveToString(Scene); // salva estado
                    Scene.StartPlay();
                    EngineLog.Info($"Play iniciado — cena '{Scene.Name}'.");
                }
                else if (_playSnapshot != null)
                {
                    SelectedObject = null;
                    SelectedLedge = null;
                    Scene.CopyFrom(SceneSerializer.LoadFromString(_playSnapshot)); // restaura
                    _playSnapshot = null;
                    History.Clear();
                    EngineLog.Info("Play encerrado — cena restaurada.");
                }
            }
        }

        private bool _snapToGrid;
        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => Set(ref _snapToGrid, value);
        }

        /// <summary>Passo do grid usado pelo snap (espelha o SceneRenderer.GridSize).</summary>
        public int GridStep { get; set; } = 32;

        public void NewScene()
        {
            _counter = 0;
            AddSceneTab(new Scene { Name = "Nova Cena" }, null);
        }

        public void SaveTo(string path)
        {
            SceneSerializer.Save(Scene, path);
            CurrentPath = path;
            if (_activeTab != null)
                _activeTab.Path = path;
            Project.RefreshScenes();
        }

        public void OpenProjectFolder(string folder)
        {
            var name = new DirectoryInfo(folder).Name;
            Project.Open(GameProject.CreateOrOpen(folder, name));
        }

        public void OpenScene(string sceneFileName)
        {
            var path = Project.ScenePath(sceneFileName);
            if (path != null && File.Exists(path))
                LoadFrom(path);
        }

        public void LoadFrom(string path)
        {
            // se a cena já está aberta em uma aba, apenas ativa
            var existing = Tabs.FirstOrDefault(t => string.Equals(t.Path, path, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                ActivateTab(existing);
                return;
            }

            AddSceneTab(SceneSerializer.Load(path), path);
        }

        public GameObject AddObject()
        {
            var obj = BuildObject();

            History.Do(new EditorAction("Adicionar objeto",
                doAction: () => { Scene.Add(obj); SelectedObject = obj; },
                undoAction: () => { if (SelectedObject == obj) SelectedObject = null; Scene.Remove(obj); }));

            return obj;
        }

        public void DeleteSelected()
        {
            if (_selectedObjects.Count > 0)
            {
                var toRemove = _selectedObjects.ToArray();
                History.Do(new EditorAction(toRemove.Length > 1 ? "Excluir objetos" : "Excluir objeto",
                    doAction: () => { SelectSingle(null); foreach (var o in toRemove) Scene.Remove(o); },
                    undoAction: () => { foreach (var o in toRemove) Scene.Add(o); SetSelection(toRemove); }));
                return;
            }

            var ledge = SelectedLedge;
            if (ledge != null)
            {
                History.Do(new EditorAction("Excluir ledge",
                    doAction: () => { if (SelectedLedge == ledge) SelectedLedge = null; Scene.RemoveLedge(ledge); },
                    undoAction: () => { Scene.AddLedge(ledge); SelectedLedge = ledge; }));
            }
        }

        // ---- Transformação em grupo (multisseleção) ----

        public readonly record struct TransformState(Vector2 Position, float Rotation, Vector2 Scale);

        public static TransformState Capture(GameObject obj)
            => new(obj.Transform.Position, obj.Transform.Rotation, obj.Transform.Scale);

        private static void ApplyStates(GameObject[] objects, TransformState[] states)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].Transform.Position = states[i].Position;
                objects[i].Transform.Rotation = states[i].Rotation;
                objects[i].Transform.Scale = states[i].Scale;
            }
        }

        /// <summary>Registra uma transformação de grupo concluída (posições/rotações/escalas já aplicadas).</summary>
        public void PushGroupTransform(GameObject[] objects, TransformState[] before, TransformState[] after)
        {
            History.Push(new EditorAction(objects.Length > 1 ? "Transformar grupo" : "Transformar objeto",
                doAction: () => ApplyStates(objects, after),
                undoAction: () => ApplyStates(objects, before)));
        }

        // ---- desenho de ledges ----

        public void AddLedgePoint(Vector2 world)
        {
            if (_pendingLedge == null)
            {
                _pendingLedge = new Ledge { OneWay = NewLedgeOneWay, Name = "Ledge " + (Scene.Ledges.Count + 1) };
                Scene.AddLedge(_pendingLedge);
            }

            _pendingLedge.AddPoint(world);
        }

        public void FinishLedge()
        {
            var ledge = _pendingLedge;
            _pendingLedge = null;
            if (ledge == null)
                return;

            if (ledge.Points.Count < 2)
            {
                Scene.RemoveLedge(ledge); // ledge degenerada
                return;
            }

            // já está na cena; registra para poder desfazer/refazer
            History.Push(new EditorAction("Adicionar ledge",
                doAction: () => Scene.AddLedge(ledge),
                undoAction: () => { if (SelectedLedge == ledge) SelectedLedge = null; Scene.RemoveLedge(ledge); }));

            SelectedLedge = ledge;
        }

        public void CancelLedge()
        {
            if (_pendingLedge == null)
                return;

            Scene.RemoveLedge(_pendingLedge);
            _pendingLedge = null;
        }

        /// <summary>Exclui a última ledge adicionada à cena (reversível).</summary>
        public void DeleteLastLedge()
        {
            if (Scene.Ledges.Count == 0)
                return;

            var ledge = Scene.Ledges[Scene.Ledges.Count - 1];
            History.Do(new EditorAction("Excluir última ledge",
                doAction: () => { if (SelectedLedge == ledge) SelectedLedge = null; Scene.RemoveLedge(ledge); },
                undoAction: () => Scene.AddLedge(ledge)));
        }

        /// <summary>Seleciona a ledge mais próxima do ponto (em mundo), dentro do limite.</summary>
        public bool TrySelectLedgeAt(Vector2 world, float maxDistance)
        {
            Ledge? nearest = null;
            float best = maxDistance;

            foreach (var ledge in Scene.Ledges)
            {
                float d = ledge.DistanceTo(world);
                if (d <= best)
                {
                    best = d;
                    nearest = ledge;
                }
            }

            if (nearest != null)
            {
                SelectedLedge = nearest;
                return true;
            }

            return false;
        }

        /// <summary>Cria um tilemap em branco a partir de um tileset (imagem), pronto para pintar.</summary>
        public void CreateTilemap(string imagePath, int tileWidth, int tileHeight, int columns, int rows)
        {
            var map = new DreamBit.Engine.Tilemap.Tilemap { TileWidth = tileWidth, TileHeight = tileHeight };
            map.Tilesets.Add(new Tileset
            {
                FirstGid = 1,
                ResolvedImagePath = imagePath,
                Columns = System.Math.Max(1, columns),
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                TileCount = System.Math.Max(1, columns) * System.Math.Max(1, rows)
            });

            var obj = new GameObject(Path.GetFileNameWithoutExtension(imagePath) + " (tilemap)");
            obj.AddComponent(new TilemapRenderer { Map = map, Edited = true });

            History.Do(new EditorAction("Novo tilemap",
                doAction: () => { Scene.Add(obj); SelectedObject = obj; },
                undoAction: () => { if (SelectedObject == obj) SelectedObject = null; Scene.Remove(obj); }));

            CurrentTool = EditorTool.Tilemap;
        }

        // ---- pincel de tilemap ----

        private int _brushGid = 1;
        private System.Collections.Generic.List<(int, int, int)>? _strokeBefore;
        private TileLayer? _strokeLayer;
        private TileLayer? _activeLayer;

        /// <summary>GID do tile atual do pincel (escolhido na paleta).</summary>
        public int BrushGid
        {
            get => _brushGid;
            set => Set(ref _brushGid, value);
        }

        /// <summary>Tilemap do objeto selecionado (alvo da pintura), se houver.</summary>
        public TilemapRenderer? ActiveTilemap =>
            SelectedObject?.Components.OfType<TilemapRenderer>().FirstOrDefault();

        /// <summary>Camada de tiles onde o pincel pinta.</summary>
        public TileLayer? ActiveLayer
        {
            get => _activeLayer;
            set => Set(ref _activeLayer, value);
        }

        /// <summary>Camadas do tilemap ativo (para o painel de camadas do editor).</summary>
        public System.Collections.Generic.IReadOnlyList<TileLayer> TileLayers =>
            ActiveTilemap?.Map?.Layers ?? (System.Collections.Generic.IReadOnlyList<TileLayer>)System.Array.Empty<TileLayer>();

        private TileLayer EnsureActiveLayer(DreamBit.Engine.Tilemap.Tilemap map)
        {
            if (_activeLayer != null && map.Layers.Contains(_activeLayer))
                return _activeLayer;
            ActiveLayer = map.PaintLayer();
            return _activeLayer!;
        }

        public void AddTileLayer()
        {
            var map = ActiveTilemap?.Map;
            if (map == null)
                return;
            ActiveLayer = map.AddLayer();
            ActiveTilemap!.Edited = true;
            OnPropertyChanged(nameof(TileLayers));
        }

        public void RemoveTileLayer(TileLayer layer)
        {
            var map = ActiveTilemap?.Map;
            if (map == null || map.Layers.Count <= 1)
                return;
            map.Layers.Remove(layer);
            if (_activeLayer == layer)
                ActiveLayer = map.Layers[map.Layers.Count - 1];
            ActiveTilemap!.Edited = true;
            OnPropertyChanged(nameof(TileLayers));
        }

        /// <summary>Move a camada na ordem de desenho (+1 = para cima/frente, -1 = para trás).</summary>
        public void MoveTileLayer(TileLayer layer, int direction)
        {
            var map = ActiveTilemap?.Map;
            if (map == null)
                return;
            int i = map.Layers.IndexOf(layer);
            int j = i + direction;
            if (i < 0 || j < 0 || j >= map.Layers.Count)
                return;
            map.Layers.RemoveAt(i);
            map.Layers.Insert(j, layer);
            ActiveTilemap!.Edited = true;
            OnPropertyChanged(nameof(TileLayers));
        }

        public void ToggleTileLayerVisible(TileLayer layer)
        {
            layer.Visible = !layer.Visible;
            if (ActiveTilemap != null)
                ActiveTilemap.Edited = true;
            OnPropertyChanged(nameof(TileLayers));
        }

        public void BeginPaintStroke()
        {
            var map = ActiveTilemap?.Map;
            if (map == null)
                return;

            _strokeLayer = EnsureActiveLayer(map);
            _strokeBefore = _strokeLayer.Tiles.ToList();
        }

        public void PaintAt(Vector2 world, bool erase)
        {
            var tilemap = ActiveTilemap;
            if (tilemap?.Map == null)
                return;

            var layer = _strokeLayer ?? EnsureActiveLayer(tilemap.Map);
            var (cellX, cellY) = tilemap.WorldToCell(world);
            layer.SetTile(cellX, cellY, erase ? 0 : _brushGid);
            tilemap.Edited = true;
        }

        public void EndPaintStroke()
        {
            if (_strokeLayer == null || _strokeBefore == null)
                return;

            var layer = _strokeLayer;
            var before = _strokeBefore;
            _strokeLayer = null;
            _strokeBefore = null;

            var after = layer.Tiles.ToList();
            if (after.Count == before.Count && !after.Except(before).Any())
                return; // nada mudou

            History.Push(new EditorAction("Pintar tiles",
                doAction: () => LoadLayer(layer, after),
                undoAction: () => LoadLayer(layer, before)));
        }

        private static void LoadLayer(TileLayer layer, System.Collections.Generic.List<(int X, int Y, int Gid)> tiles)
        {
            layer.Clear();
            foreach (var (x, y, gid) in tiles)
                layer.SetTile(x, y, gid);
        }

        /// <summary>Anexa um comportamento de runtime (gira no play) ao objeto selecionado.</summary>
        public void AddRotator()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<RotatorBehavior>().Any())
                return;

            var rotator = new RotatorBehavior();
            History.Do(new EditorAction("Adicionar Rotator",
                doAction: () => obj.AddComponent(rotator),
                undoAction: () => obj.RemoveComponent(rotator)));
        }

        public void AddSprite()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<SpriteRenderer>().Any())
                return;

            var sprite = new SpriteRenderer();
            History.Do(new EditorAction("Adicionar Sprite",
                doAction: () => obj.AddComponent(sprite),
                undoAction: () => obj.RemoveComponent(sprite)));
        }

        public void RemoveComponent(SceneComponent component)
        {
            var obj = SelectedObject;
            if (obj == null || component == null)
                return;

            History.Do(new EditorAction("Remover componente",
                doAction: () => obj.RemoveComponent(component),
                undoAction: () => obj.AddComponent(component)));
        }

        public void RemoveSprite()
        {
            var sprite = SelectedObject?.Components.OfType<SpriteRenderer>().FirstOrDefault();
            if (sprite != null)
                RemoveComponent(sprite);
        }

        public void RemoveRotator()
        {
            var rotator = SelectedObject?.Components.OfType<RotatorBehavior>().FirstOrDefault();
            if (rotator != null)
                RemoveComponent(rotator);
        }

        /// <summary>Importa um mapa Tiled (.tmx) como um novo objeto com TilemapRenderer.</summary>
        public void ImportTilemap(string tmxPath)
        {
            var obj = new GameObject(Path.GetFileNameWithoutExtension(tmxPath));
            obj.AddComponent(new TilemapRenderer { TmxPath = tmxPath });

            History.Do(new EditorAction("Importar tilemap",
                doAction: () => { Scene.Add(obj); SelectedObject = obj; },
                undoAction: () => { if (SelectedObject == obj) SelectedObject = null; Scene.Remove(obj); }));
        }

        public void RemoveTilemap()
        {
            var tilemap = SelectedObject?.Components.OfType<TilemapRenderer>().FirstOrDefault();
            if (tilemap != null)
                RemoveComponent(tilemap);
        }

        public void AddScript()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<ScriptComponent>().Any())
                return;

            var script = new ScriptComponent();
            History.Do(new EditorAction("Adicionar Script",
                doAction: () => obj.AddComponent(script),
                undoAction: () => obj.RemoveComponent(script)));
        }

        public void RemoveScript()
        {
            var script = SelectedObject?.Components.OfType<ScriptComponent>().FirstOrDefault();
            if (script != null)
                RemoveComponent(script);
        }

        public void AddTrigger()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<TriggerZone>().Any())
                return;

            var trigger = new TriggerZone();
            History.Do(new EditorAction("Adicionar Trigger",
                doAction: () => obj.AddComponent(trigger),
                undoAction: () => obj.RemoveComponent(trigger)));
        }

        public void RemoveTrigger()
        {
            var trigger = SelectedObject?.Components.OfType<TriggerZone>().FirstOrDefault();
            if (trigger != null)
                RemoveComponent(trigger);
        }

        public void AddFollow()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<FollowTarget>().Any())
                return;

            var follow = new FollowTarget();
            History.Do(new EditorAction("Adicionar Follow",
                doAction: () => obj.AddComponent(follow),
                undoAction: () => obj.RemoveComponent(follow)));
        }

        public void RemoveFollow()
        {
            var follow = SelectedObject?.Components.OfType<FollowTarget>().FirstOrDefault();
            if (follow != null)
                RemoveComponent(follow);
        }

        public void AddParticles()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<ParticleEmitter>().Any())
                return;

            var particles = new ParticleEmitter();
            History.Do(new EditorAction("Adicionar Particulas",
                doAction: () => obj.AddComponent(particles),
                undoAction: () => obj.RemoveComponent(particles)));
        }

        public void RemoveParticles()
        {
            var particles = SelectedObject?.Components.OfType<ParticleEmitter>().FirstOrDefault();
            if (particles != null)
                RemoveComponent(particles);
        }

        public void AddAudio()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<AudioSource>().Any())
                return;

            var audio = new AudioSource();
            History.Do(new EditorAction("Adicionar Audio",
                doAction: () => obj.AddComponent(audio),
                undoAction: () => obj.RemoveComponent(audio)));
        }

        public void RemoveAudio()
        {
            var audio = SelectedObject?.Components.OfType<AudioSource>().FirstOrDefault();
            if (audio != null)
                RemoveComponent(audio);
        }

        public void AddPlatformer()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<PlatformerController>().Any())
                return;

            var platformer = new PlatformerController();
            History.Do(new EditorAction("Adicionar Platformer",
                doAction: () => obj.AddComponent(platformer),
                undoAction: () => obj.RemoveComponent(platformer)));
        }

        public void RemovePlatformer()
        {
            var platformer = SelectedObject?.Components.OfType<PlatformerController>().FirstOrDefault();
            if (platformer != null)
                RemoveComponent(platformer);
        }

        public void AddAnimator()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<SpriteAnimator>().Any())
                return;

            var animator = new SpriteAnimator();
            History.Do(new EditorAction("Adicionar Animator",
                doAction: () => obj.AddComponent(animator),
                undoAction: () => obj.RemoveComponent(animator)));
        }

        public void RemoveAnimator()
        {
            var animator = SelectedObject?.Components.OfType<SpriteAnimator>().FirstOrDefault();
            if (animator != null)
                RemoveComponent(animator);
        }

        public void AddBone()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<Bone>().Any())
                return;

            var bone = new Bone();
            bone.CaptureRestPose();
            History.Do(new EditorAction("Adicionar Bone",
                doAction: () => obj.AddComponent(bone),
                undoAction: () => obj.RemoveComponent(bone)));
        }

        public void RemoveBone()
        {
            var bone = SelectedObject?.Components.OfType<Bone>().FirstOrDefault();
            if (bone != null)
                RemoveComponent(bone);
        }

        public void AddCollider()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<BoxCollider>().Any())
                return;

            var collider = new BoxCollider();
            History.Do(new EditorAction("Adicionar Box Collider",
                doAction: () => obj.AddComponent(collider),
                undoAction: () => obj.RemoveComponent(collider)));
        }

        public void RemoveCollider()
        {
            var collider = SelectedObject?.Components.OfType<BoxCollider>().FirstOrDefault();
            if (collider != null)
                RemoveComponent(collider);
        }

        public void AddAnimatorController()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<AnimatorController>().Any())
                return;

            var ctrl = new AnimatorController();
            History.Do(new EditorAction("Adicionar Animator Controller",
                doAction: () => obj.AddComponent(ctrl),
                undoAction: () => obj.RemoveComponent(ctrl)));
        }

        public void RemoveAnimatorController()
        {
            var ctrl = SelectedObject?.Components.OfType<AnimatorController>().FirstOrDefault();
            if (ctrl != null)
                RemoveComponent(ctrl);
        }

        public void AddTween()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<TweenComponent>().Any())
                return;

            var tween = new TweenComponent();
            History.Do(new EditorAction("Adicionar Tween",
                doAction: () => obj.AddComponent(tween),
                undoAction: () => obj.RemoveComponent(tween)));
        }

        public void RemoveTween()
        {
            var tween = SelectedObject?.Components.OfType<TweenComponent>().FirstOrDefault();
            if (tween != null)
                RemoveComponent(tween);
        }

        public void AddText()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<TextRenderer>().Any())
                return;

            var text = new TextRenderer();
            History.Do(new EditorAction("Adicionar Text",
                doAction: () => obj.AddComponent(text),
                undoAction: () => obj.RemoveComponent(text)));
        }

        public void RemoveText()
        {
            var text = SelectedObject?.Components.OfType<TextRenderer>().FirstOrDefault();
            if (text != null)
                RemoveComponent(text);
        }

        public void AddCamera()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<CameraComponent>().Any())
                return;

            var camera = new CameraComponent();
            History.Do(new EditorAction("Adicionar Camera",
                doAction: () => obj.AddComponent(camera),
                undoAction: () => obj.RemoveComponent(camera)));
        }

        public void RemoveCamera()
        {
            var camera = SelectedObject?.Components.OfType<CameraComponent>().FirstOrDefault();
            if (camera != null)
                RemoveComponent(camera);
        }

        public void AddUiAnchor()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<UiAnchor>().Any())
                return;

            var anchor = new UiAnchor();
            History.Do(new EditorAction("Adicionar UI Anchor",
                doAction: () => obj.AddComponent(anchor),
                undoAction: () => obj.RemoveComponent(anchor)));
        }

        public void RemoveUiAnchor()
        {
            var anchor = SelectedObject?.Components.OfType<UiAnchor>().FirstOrDefault();
            if (anchor != null)
                RemoveComponent(anchor);
        }

        public void AddUiButton()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<UiButton>().Any())
                return;

            // Botão só faz sentido em HUD: marca o objeto como fixo na tela ao adicionar.
            var button = new UiButton();
            bool wasScreenSpace = obj.ScreenSpace;
            History.Do(new EditorAction("Adicionar UI Button",
                doAction: () => { obj.ScreenSpace = true; obj.AddComponent(button); },
                undoAction: () => { obj.RemoveComponent(button); obj.ScreenSpace = wasScreenSpace; }));
        }

        public void RemoveUiButton()
        {
            var button = SelectedObject?.Components.OfType<UiButton>().FirstOrDefault();
            if (button != null)
                RemoveComponent(button);
        }

        public void AddParallax()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<ParallaxLayer>().Any())
                return;

            var parallax = new ParallaxLayer();
            History.Do(new EditorAction("Adicionar Parallax",
                doAction: () => obj.AddComponent(parallax),
                undoAction: () => obj.RemoveComponent(parallax)));
        }

        public void RemoveParallax()
        {
            var parallax = SelectedObject?.Components.OfType<ParallaxLayer>().FirstOrDefault();
            if (parallax != null)
                RemoveComponent(parallax);
        }

        public void AddTimer()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<TimerComponent>().Any())
                return;

            var timer = new TimerComponent();
            History.Do(new EditorAction("Adicionar Timer",
                doAction: () => obj.AddComponent(timer),
                undoAction: () => obj.RemoveComponent(timer)));
        }

        public void RemoveTimer()
        {
            var timer = SelectedObject?.Components.OfType<TimerComponent>().FirstOrDefault();
            if (timer != null)
                RemoveComponent(timer);
        }

        public void AddUiLayout()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<UiLayout>().Any())
                return;

            var layout = new UiLayout();
            History.Do(new EditorAction("Adicionar UI Layout",
                doAction: () => obj.AddComponent(layout),
                undoAction: () => obj.RemoveComponent(layout)));
        }

        public void RemoveUiLayout()
        {
            var layout = SelectedObject?.Components.OfType<UiLayout>().FirstOrDefault();
            if (layout != null)
                RemoveComponent(layout);
        }

        public void AddUiSlider() => AddSingle(() => new UiSlider(), "Adicionar UI Slider");
        public void RemoveUiSlider() => RemoveSingle<UiSlider>();
        public void AddUiToggle() => AddSingle(() => new UiToggle(), "Adicionar UI Toggle");
        public void RemoveUiToggle() => RemoveSingle<UiToggle>();
        public void AddUiProgressBar() => AddSingle(() => new UiProgressBar(), "Adicionar UI Progress Bar");
        public void RemoveUiProgressBar() => RemoveSingle<UiProgressBar>();
        public void AddUiTextField() => AddSingle(() => new UiTextField(), "Adicionar UI Text Field");
        public void RemoveUiTextField() => RemoveSingle<UiTextField>();
        public void AddUiNavigator() => AddSingle(() => new UiNavigator(), "Adicionar UI Navigator");
        public void RemoveUiNavigator() => RemoveSingle<UiNavigator>();
        public void AddUiScrollView() => AddSingle(() => new UiScrollView(), "Adicionar UI Scroll View");
        public void RemoveUiScrollView() => RemoveSingle<UiScrollView>();

        private void AddSingle<T>(System.Func<T> create, string label) where T : SceneComponent
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<T>().Any())
                return;
            var comp = create();
            History.Do(new EditorAction(label,
                doAction: () => obj.AddComponent(comp),
                undoAction: () => obj.RemoveComponent(comp)));
        }

        private void RemoveSingle<T>() where T : SceneComponent
        {
            var comp = SelectedObject?.Components.OfType<T>().FirstOrDefault();
            if (comp != null)
                RemoveComponent(comp);
        }

        public void AddRigidbody()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<Rigidbody2D>().Any())
                return;

            var rb = new Rigidbody2D();
            History.Do(new EditorAction("Adicionar Rigidbody 2D",
                doAction: () => obj.AddComponent(rb),
                undoAction: () => obj.RemoveComponent(rb)));
        }

        public void AddPropertyAnimator()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<PropertyAnimator>().Any())
                return;

            var pa = new PropertyAnimator();
            pa.Tracks.Add(new PropertyTrack { Channel = AnimChannel.PositionY, Keys = { new AnimKey(0f, 0f), new AnimKey(1f, 100f) } });
            History.Do(new EditorAction("Adicionar Property Animator",
                doAction: () => obj.AddComponent(pa),
                undoAction: () => obj.RemoveComponent(pa)));
        }

        public void RemovePropertyAnimator()
        {
            var pa = SelectedObject?.Components.OfType<PropertyAnimator>().FirstOrDefault();
            if (pa != null)
                RemoveComponent(pa);
        }

        public void RemoveRigidbody()
        {
            var rb = SelectedObject?.Components.OfType<Rigidbody2D>().FirstOrDefault();
            if (rb != null)
                RemoveComponent(rb);
        }

        public void AddMessageListener()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<MessageListener>().Any())
                return;

            var listener = new MessageListener();
            History.Do(new EditorAction("Adicionar Message Listener",
                doAction: () => obj.AddComponent(listener),
                undoAction: () => obj.RemoveComponent(listener)));
        }

        public void RemoveMessageListener()
        {
            var listener = SelectedObject?.Components.OfType<MessageListener>().FirstOrDefault();
            if (listener != null)
                RemoveComponent(listener);
        }

        public void AddSkeleton()
        {
            var obj = SelectedObject;
            if (obj == null || obj.Components.OfType<SkeletonAnimator>().Any())
                return;

            var skeleton = new SkeletonAnimator();
            History.Do(new EditorAction("Adicionar Skeleton Animator",
                doAction: () => obj.AddComponent(skeleton),
                undoAction: () => obj.RemoveComponent(skeleton)));
        }

        public void RemoveSkeleton()
        {
            var skeleton = SelectedObject?.Components.OfType<SkeletonAnimator>().FirstOrDefault();
            if (skeleton != null)
                RemoveComponent(skeleton);
        }

        /// <summary>Duplica os objetos selecionados (com seus componentes) — reversível.</summary>
        public void DuplicateSelected()
        {
            if (_selectedObjects.Count == 0)
                return;

            var clones = _selectedObjects.Select(Clone).ToArray();
            History.Do(new EditorAction(clones.Length > 1 ? "Duplicar objetos" : "Duplicar objeto",
                doAction: () => { foreach (var c in clones) Scene.Add(c); SetSelection(clones); },
                undoAction: () => { SelectSingle(null); foreach (var c in clones) Scene.Remove(c); }));
        }

        private static GameObject Clone(GameObject src)
        {
            var clone = new GameObject(src.Name + " (cópia)") { Tag = src.Tag, SortOrder = src.SortOrder, RenderLayer = src.RenderLayer, ScreenSpace = src.ScreenSpace };
            clone.Transform.Position = src.Transform.Position + new Vector2(16, 16);
            clone.Transform.Rotation = src.Transform.Rotation;
            clone.Transform.Scale = src.Transform.Scale;

            foreach (var component in src.Components)
            {
                switch (component)
                {
                    case SpriteRenderer s:
                        clone.AddComponent(new SpriteRenderer { Size = s.Size, Color = s.Color, TexturePath = s.TexturePath, SourceRect = s.SourceRect,
                            ChromaKeyEnabled = s.ChromaKeyEnabled, ChromaAuto = s.ChromaAuto, ChromaColor = s.ChromaColor, ChromaTolerance = s.ChromaTolerance });
                        break;
                    case RotatorBehavior r:
                        clone.AddComponent(new RotatorBehavior { Speed = r.Speed });
                        break;
                    case SpriteAnimator a:
                        var animClone = new SpriteAnimator
                        {
                            TexturePath = a.TexturePath, FrameWidth = a.FrameWidth, FrameHeight = a.FrameHeight,
                            FrameCount = a.FrameCount, Fps = a.Fps, Loop = a.Loop, Size = a.Size,
                            ChromaKeyEnabled = a.ChromaKeyEnabled, ChromaAuto = a.ChromaAuto, ChromaColor = a.ChromaColor, ChromaTolerance = a.ChromaTolerance,
                            FlipX = a.FlipX
                        };
                        animClone.SetEvents(a.Events);
                        animClone.SetFrames(a.Frames);
                        animClone.SetClips(a.Clips.Select(c => new SpriteClip(c.Name, c.Frames, c.Fps, c.Loop)));
                        if (a.CurrentClip != null) animClone.Play(a.CurrentClip);
                        clone.AddComponent(animClone);
                        break;
                    case SpriteAnimatorController sac:
                        clone.AddComponent(new SpriteAnimatorController
                        {
                            IdleClip = sac.IdleClip, WalkClip = sac.WalkClip, JumpClip = sac.JumpClip,
                            AttackClip = sac.AttackClip, AttackAction = sac.AttackAction,
                            FlipByVelocity = sac.FlipByVelocity, ArtFacesRight = sac.ArtFacesRight
                        });
                        break;
                    case TilemapRenderer t:
                        clone.AddComponent(new TilemapRenderer { TmxPath = t.TmxPath, Solid = t.Solid, SolidLayer = t.SolidLayer });
                        break;
                    case PlatformerController p:
                        clone.AddComponent(new PlatformerController
                        {
                            Gravity = p.Gravity, HalfHeight = p.HalfHeight, HalfWidth = p.HalfWidth,
                            HorizontalSpeed = p.HorizontalSpeed,
                            UseKeyboard = p.UseKeyboard, MoveSpeed = p.MoveSpeed, JumpSpeed = p.JumpSpeed
                        });
                        break;
                    case BoxCollider bc:
                        clone.AddComponent(new BoxCollider { Size = bc.Size, Offset = bc.Offset });
                        break;
                    case SceneExit se:
                        clone.AddComponent(new SceneExit { Size = se.Size, TargetTag = se.TargetTag, TargetScene = se.TargetScene });
                        break;
                    case TextRenderer tr:
                        clone.AddComponent(new TextRenderer { Text = tr.Text, LocKey = tr.LocKey, Color = tr.Color, PixelSize = tr.PixelSize, ScreenSpace = tr.ScreenSpace });
                        break;
                    case TweenComponent tw:
                        clone.AddComponent(new TweenComponent
                        {
                            Channel = tw.Channel, From = tw.From, To = tw.To, Duration = tw.Duration,
                            Easing = tw.Easing, Loop = tw.Loop, PlayOnStart = tw.PlayOnStart
                        });
                        break;
                    case AnimatorController ac:
                        clone.AddComponent(new AnimatorController { IdleClip = ac.IdleClip, WalkClip = ac.WalkClip, JumpClip = ac.JumpClip });
                        break;
                    case UiAnchor an:
                        clone.AddComponent(new UiAnchor { Anchor = an.Anchor, OffsetX = an.OffsetX, OffsetY = an.OffsetY });
                        break;
                    case UiButton bt:
                        clone.AddComponent(new UiButton
                        {
                            Width = bt.Width, Height = bt.Height,
                            Normal = bt.Normal, Hover = bt.Hover, Pressed = bt.Pressed, SendOnClick = bt.SendOnClick
                        });
                        break;
                    case ParallaxLayer px:
                        clone.AddComponent(new ParallaxLayer { FactorX = px.FactorX, FactorY = px.FactorY });
                        break;
                    case TimerComponent tm:
                        clone.AddComponent(new TimerComponent
                        {
                            Duration = tm.Duration, Repeat = tm.Repeat, AutoStart = tm.AutoStart,
                            SendOnElapsed = tm.SendOnElapsed, StartOn = tm.StartOn
                        });
                        break;
                    case UiLayout ul:
                        clone.AddComponent(new UiLayout { Direction = ul.Direction, Spacing = ul.Spacing });
                        break;
                    case UiSlider sl:
                        clone.AddComponent(new UiSlider
                        {
                            Value = sl.Value, Width = sl.Width, Height = sl.Height,
                            Track = sl.Track, Fill = sl.Fill, Knob = sl.Knob, BusTarget = sl.BusTarget, SendOnChange = sl.SendOnChange
                        });
                        break;
                    case UiToggle tg:
                        clone.AddComponent(new UiToggle { IsOn = tg.IsOn, Size = tg.Size, Box = tg.Box, Check = tg.Check, SendOnChange = tg.SendOnChange });
                        break;
                    case UiProgressBar pb:
                        clone.AddComponent(new UiProgressBar { Value = pb.Value, Width = pb.Width, Height = pb.Height, Track = pb.Track, Fill = pb.Fill });
                        break;
                    case UiTextField tf:
                        clone.AddComponent(new UiTextField
                        {
                            Text = tf.Text, Placeholder = tf.Placeholder, Width = tf.Width, Height = tf.Height,
                            PixelSize = tf.PixelSize, MaxLength = tf.MaxLength, SendOnSubmit = tf.SendOnSubmit
                        });
                        break;
                    case UiNavigator nav:
                        clone.AddComponent(new UiNavigator { AutoFocusFirst = nav.AutoFocusFirst });
                        break;
                    case UiScrollView sv:
                        clone.AddComponent(new UiScrollView { Width = sv.Width, Height = sv.Height, Spacing = sv.Spacing, ScrollSpeed = sv.ScrollSpeed, Background = sv.Background });
                        break;
                    case Rigidbody2D rb:
                        clone.AddComponent(new Rigidbody2D
                        {
                            Kind = rb.Kind, Shape = rb.Shape, Width = rb.Width, Height = rb.Height, Radius = rb.Radius,
                            Density = rb.Density, Friction = rb.Friction, Restitution = rb.Restitution, FixedRotation = rb.FixedRotation,
                            CollisionCategory = rb.CollisionCategory, CollidesWith = rb.CollidesWith
                        });
                        break;
                    case PropertyAnimator pa:
                        var paClone = new PropertyAnimator { Duration = pa.Duration, Loop = pa.Loop, PlayOnStart = pa.PlayOnStart };
                        foreach (var tr in pa.Tracks)
                        {
                            var t2 = new PropertyTrack { Channel = tr.Channel, Easing = tr.Easing };
                            t2.Keys.AddRange(tr.Keys);
                            paClone.Tracks.Add(t2);
                        }
                        clone.AddComponent(paClone);
                        break;
                    case CameraComponent cam:
                        clone.AddComponent(new CameraComponent
                        {
                            TargetTag = cam.TargetTag, DeadzoneWidth = cam.DeadzoneWidth, DeadzoneHeight = cam.DeadzoneHeight,
                            SmoothTime = cam.SmoothTime, Zoom = cam.Zoom,
                            UseBounds = cam.UseBounds, BoundsMin = cam.BoundsMin, BoundsMax = cam.BoundsMax
                        });
                        break;
                    case AudioSource au:
                        clone.AddComponent(new AudioSource
                        {
                            SoundPath = au.SoundPath, Volume = au.Volume, PlayOnStart = au.PlayOnStart, Loop = au.Loop, Bus = au.Bus,
                            Spatial = au.Spatial, MaxDistance = au.MaxDistance
                        });
                        break;
                    case AudioListener:
                        clone.AddComponent(new AudioListener());
                        break;
                    case Light2D li:
                        clone.AddComponent(new Light2D { Radius = li.Radius, Intensity = li.Intensity, Color = li.Color });
                        break;
                    case AmbientLight al:
                        clone.AddComponent(new AmbientLight { Color = al.Color });
                        break;
                    case TopDownController td:
                        clone.AddComponent(new TopDownController { MoveSpeed = td.MoveSpeed, UseKeyboard = td.UseKeyboard, HalfWidth = td.HalfWidth, HalfHeight = td.HalfHeight });
                        break;
                    case Health hp:
                        clone.AddComponent(new Health { Max = hp.Max, InvulnTime = hp.InvulnTime, SendOnHit = hp.SendOnHit, SendOnDeath = hp.SendOnDeath, DestroyOnDeath = hp.DestroyOnDeath });
                        break;
                    case Hurtbox hb:
                        clone.AddComponent(new Hurtbox { Width = hb.Width, Height = hb.Height, Offset = hb.Offset, Team = hb.Team });
                        break;
                    case Hitbox hx:
                        clone.AddComponent(new Hitbox { Width = hx.Width, Height = hx.Height, Offset = hx.Offset, Team = hx.Team, Damage = hx.Damage, ActiveTime = hx.ActiveTime, ActivateOn = hx.ActivateOn });
                        break;
                    case SpriteFlash sf:
                        clone.AddComponent(new SpriteFlash { FlashColor = sf.FlashColor, Duration = sf.Duration });
                        break;
                    case NavChaser nc:
                        clone.AddComponent(new NavChaser
                        {
                            TargetTag = nc.TargetTag, Speed = nc.Speed, RepathInterval = nc.RepathInterval,
                            ArriveRadius = nc.ArriveRadius, AllowDiagonal = nc.AllowDiagonal
                        });
                        break;
                    case ParticleEmitter pe:
                        clone.AddComponent(new ParticleEmitter
                        {
                            EmitRate = pe.EmitRate, Lifetime = pe.Lifetime, Speed = pe.Speed, Spread = pe.Spread,
                            Size = pe.Size, GravityY = pe.GravityY, Color = pe.Color
                        });
                        break;
                    case FollowTarget ft:
                        clone.AddComponent(new FollowTarget { TargetId = ft.TargetId, Speed = ft.Speed });
                        break;
                    case TriggerZone tz:
                        clone.AddComponent(new TriggerZone
                        {
                            Size = tz.Size, Color = tz.Color,
                            TargetTag = tz.TargetTag, DestroyOnEnter = tz.DestroyOnEnter, SendOnEnter = tz.SendOnEnter
                        });
                        break;
                    case MessageListener ml:
                        clone.AddComponent(new MessageListener { Message = ml.Message, Reaction = ml.Reaction });
                        break;
                    case ScriptComponent sc:
                        clone.AddComponent(new ScriptComponent { Source = sc.Source, SourcePath = sc.SourcePath });
                        break;
                    case Bone bone:
                        clone.AddComponent(new Bone
                        {
                            Length = bone.Length, RestPosition = bone.RestPosition,
                            RestRotation = bone.RestRotation, RestScale = bone.RestScale,
                            HasRestPose = bone.HasRestPose
                        });
                        break;
                    case SkeletonAnimator skel:
                        var skelClone = new SkeletonAnimator();
                        bool firstClip = true;
                        foreach (var clip in skel.Clips)
                        {
                            if (firstClip) { skelClone.CurrentClip.Name = clip.Name; firstClip = false; }
                            else skelClone.AddClip(clip.Name);
                            skelClone.Duration = clip.Duration;
                            skelClone.Loop = clip.Loop;
                            skelClone.Easing = clip.Easing;
                            skelClone.SetEvents(clip.Events);
                            foreach (var kf in clip.Keyframes)
                                skelClone.AddKeyframe(new DreamBit.Engine.Animation.PoseKeyframe(
                                    kf.Time, new System.Collections.Generic.Dictionary<string, DreamBit.Engine.Animation.BonePose>(kf.Bones)));
                        }
                        skelClone.CurrentClipName = skel.CurrentClipName;
                        clone.AddComponent(skelClone);
                        break;
                }
            }

            return clone;
        }

        public void SaveSelectedAsPrefab(string path)
        {
            if (SelectedObject != null)
                SceneSerializer.SaveObject(SelectedObject, path);
        }

        public void InsertPrefab(string path)
        {
            var obj = SceneSerializer.LoadPrefab(path);
            History.Do(new EditorAction("Inserir prefab",
                doAction: () => { Scene.Add(obj); SelectedObject = obj; },
                undoAction: () => { if (SelectedObject == obj) SelectedObject = null; Scene.Remove(obj); }));
        }

        private System.Collections.Generic.List<GameObject> _clipboard = new();

        public void CopySelected()
        {
            if (_selectedObjects.Count > 0)
                _clipboard = _selectedObjects.Select(Clone).ToList();
        }

        public void Paste()
        {
            if (_clipboard.Count == 0)
                return;

            var pasted = _clipboard.Select(Clone).ToArray(); // clona de novo: cada colar é independente
            History.Do(new EditorAction(pasted.Length > 1 ? "Colar objetos" : "Colar objeto",
                doAction: () => { foreach (var o in pasted) Scene.Add(o); SetSelection(pasted); },
                undoAction: () => { SelectSingle(null); foreach (var o in pasted) Scene.Remove(o); }));
        }

        /// <summary>Move todos os objetos selecionados por um delta (setas) — reversível.</summary>
        public void Nudge(Vector2 delta)
        {
            if (_selectedObjects.Count == 0)
                return;

            var objects = _selectedObjects.ToArray();
            var before = objects.Select(Capture).ToArray();
            foreach (var obj in objects)
                obj.Transform.Position += delta;
            PushGroupTransform(objects, before, objects.Select(Capture).ToArray());
        }

        /// <summary>Centro (mundo) da seleção atual, para focar a câmera.</summary>
        public Vector2 SelectionCenter()
        {
            if (_selectedObjects.Count == 0)
                return Camera.Position;

            var sum = Vector2.Zero;
            foreach (var obj in _selectedObjects)
                sum += obj.Transform.WorldPosition;
            return sum / _selectedObjects.Count;
        }

        /// <summary>Registra um arraste concluído no histórico (a posição já foi aplicada).</summary>
        public void PushMove(GameObject obj, Vector2 from, Vector2 to)
        {
            if (from == to)
                return;

            History.Push(new EditorAction("Mover objeto",
                doAction: () => obj.Transform.Position = to,
                undoAction: () => obj.Transform.Position = from));
        }

        /// <summary>Registra uma rotação concluída (via gizmo) no histórico.</summary>
        public void PushRotation(GameObject obj, float from, float to)
        {
            if (System.Math.Abs(from - to) < 0.0001f)
                return;

            History.Push(new EditorAction("Rotacionar objeto",
                doAction: () => obj.Transform.Rotation = to,
                undoAction: () => obj.Transform.Rotation = from));
        }

        /// <summary>Registra uma escala concluída (via gizmo) no histórico.</summary>
        public void PushScale(GameObject obj, Vector2 from, Vector2 to)
        {
            if (from == to)
                return;

            History.Push(new EditorAction("Escalar objeto",
                doAction: () => obj.Transform.Scale = to,
                undoAction: () => obj.Transform.Scale = from));
        }

        /// <summary>Aplica uma imagem do projeto: como textura do sprite selecionado,
        /// ou cria um novo objeto com esse sprite. Reversível.</summary>
        public void UseAsset(string assetFullPath)
        {
            var sprite = SelectedObject?.Components.OfType<SpriteRenderer>().FirstOrDefault();
            if (sprite != null)
            {
                var old = sprite.TexturePath;
                History.Do(new EditorAction("Definir textura",
                    doAction: () => sprite.TexturePath = assetFullPath,
                    undoAction: () => sprite.TexturePath = old));
                return;
            }

            var obj = BuildObject();
            obj.Components.OfType<SpriteRenderer>().First().TexturePath = assetFullPath;
            History.Do(new EditorAction("Objeto a partir do asset",
                doAction: () => { Scene.Add(obj); SelectedObject = obj; },
                undoAction: () => { if (SelectedObject == obj) SelectedObject = null; Scene.Remove(obj); }));
        }

        // ---- carimbo de atlas ----

        private bool _stampMode;

        /// <summary>Carimbo atual selecionado (textura + recorte), ou null.</summary>
        public (string Path, Rectangle Source)? CurrentStamp { get; private set; }

        /// <summary>Quando ativo, clicar no canvas carimba o <see cref="CurrentStamp"/> no ponto.</summary>
        public bool StampMode
        {
            get => _stampMode;
            set => Set(ref _stampMode, value);
        }

        public bool HasStamp => CurrentStamp != null;

        /// <summary>Define o carimbo atual e liga o modo carimbo (clicar na cena posiciona).</summary>
        public void SetCurrentStamp(string texturePath, Rectangle source)
        {
            CurrentStamp = (texturePath, source);
            StampMode = true;
            OnPropertyChanged(nameof(HasStamp));
        }

        public void ClearStamp()
        {
            CurrentStamp = null;
            StampMode = false;
            OnPropertyChanged(nameof(HasStamp));
        }

        /// <summary>Carimba o carimbo atual no ponto (mundo). Usado pelo clique no canvas.</summary>
        public GameObject? StampCurrentAt(Vector2 worldPosition)
        {
            if (CurrentStamp is not { } stamp)
                return null;
            return StampFromAtlas(stamp.Path, stamp.Source, worldPosition);
        }

        /// <summary>
        /// Carimba um recorte de um atlas (textura + source rect) como um novo objeto na
        /// posição informada (mundo). O tamanho do sprite espelha o recorte. Reversível.
        /// </summary>
        public GameObject StampFromAtlas(string texturePath, Rectangle source, Vector2 worldPosition)
        {
            var name = Path.GetFileNameWithoutExtension(texturePath);
            var obj = new GameObject($"{name} [{source.X},{source.Y}]");
            obj.Transform.Position = worldPosition;
            obj.AddComponent(new SpriteRenderer
            {
                TexturePath = texturePath,
                SourceRect = source,
                Size = new Vector2(source.Width, source.Height)
            });

            History.Do(new EditorAction("Carimbar do atlas",
                doAction: () => { Scene.Add(obj); SelectedObject = obj; },
                undoAction: () => { if (SelectedObject == obj) SelectedObject = null; Scene.Remove(obj); }));

            return obj;
        }

        private GameObject BuildObject()
        {
            var obj = new GameObject($"GameObject {++_counter}");
            obj.AddComponent(new SpriteRenderer
            {
                Size = new Vector2(96, 64),
                Color = new Color(70 + _counter * 25 % 150, 130, 200)
            });
            return obj;
        }

        private void SeedSampleScene()
        {
            var a = AddObject();
            a.Transform.Position = new Vector2(-140, -40);

            var b = AddObject();
            b.Transform.Position = new Vector2(120, 60);
            b.Transform.Rotation = 0.3f;
            b.AddComponent(new RotatorBehavior { Speed = 1.5f });

            SelectedObject = a;
        }
    }
}
