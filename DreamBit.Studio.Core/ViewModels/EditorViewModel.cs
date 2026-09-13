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

        /// <summary>GID do tile atual do pincel (escolhido na paleta).</summary>
        public int BrushGid
        {
            get => _brushGid;
            set => Set(ref _brushGid, value);
        }

        /// <summary>Tilemap do objeto selecionado (alvo da pintura), se houver.</summary>
        public TilemapRenderer? ActiveTilemap =>
            SelectedObject?.Components.OfType<TilemapRenderer>().FirstOrDefault();

        public void BeginPaintStroke()
        {
            var map = ActiveTilemap?.Map;
            if (map == null)
                return;

            _strokeLayer = map.PaintLayer();
            _strokeBefore = _strokeLayer.Tiles.ToList();
        }

        public void PaintAt(Vector2 world, bool erase)
        {
            var tilemap = ActiveTilemap;
            if (tilemap == null)
                return;

            var (cellX, cellY) = tilemap.WorldToCell(world);
            tilemap.Paint(cellX, cellY, erase ? 0 : _brushGid);
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
            var clone = new GameObject(src.Name + " (cópia)") { Tag = src.Tag };
            clone.Transform.Position = src.Transform.Position + new Vector2(16, 16);
            clone.Transform.Rotation = src.Transform.Rotation;
            clone.Transform.Scale = src.Transform.Scale;

            foreach (var component in src.Components)
            {
                switch (component)
                {
                    case SpriteRenderer s:
                        clone.AddComponent(new SpriteRenderer { Size = s.Size, Color = s.Color, TexturePath = s.TexturePath, SourceRect = s.SourceRect });
                        break;
                    case RotatorBehavior r:
                        clone.AddComponent(new RotatorBehavior { Speed = r.Speed });
                        break;
                    case SpriteAnimator a:
                        var animClone = new SpriteAnimator
                        {
                            TexturePath = a.TexturePath, FrameWidth = a.FrameWidth, FrameHeight = a.FrameHeight,
                            FrameCount = a.FrameCount, Fps = a.Fps, Loop = a.Loop, Size = a.Size
                        };
                        animClone.SetEvents(a.Events);
                        clone.AddComponent(animClone);
                        break;
                    case TilemapRenderer t:
                        clone.AddComponent(new TilemapRenderer { TmxPath = t.TmxPath });
                        break;
                    case PlatformerController p:
                        clone.AddComponent(new PlatformerController
                        {
                            Gravity = p.Gravity, HalfHeight = p.HalfHeight, HorizontalSpeed = p.HorizontalSpeed,
                            UseKeyboard = p.UseKeyboard, MoveSpeed = p.MoveSpeed, JumpSpeed = p.JumpSpeed
                        });
                        break;
                    case AudioSource au:
                        clone.AddComponent(new AudioSource
                        {
                            SoundPath = au.SoundPath, Volume = au.Volume, PlayOnStart = au.PlayOnStart, Loop = au.Loop
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
                            TargetTag = tz.TargetTag, DestroyOnEnter = tz.DestroyOnEnter
                        });
                        break;
                    case ScriptComponent sc:
                        clone.AddComponent(new ScriptComponent { Source = sc.Source });
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
