using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Editing;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Notification;
using DreamBit.Engine.Project;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
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

            AddObjectCommand = new RelayCommand(() => AddObject());
            DeleteObjectCommand = new RelayCommand(DeleteSelected, () => SelectedObject != null || SelectedLedge != null);
            AddRotatorCommand = new RelayCommand(AddRotator, () => SelectedObject != null);
            UndoCommand = new RelayCommand(History.Undo, () => History.CanUndo);
            RedoCommand = new RelayCommand(History.Redo, () => History.CanRedo);
            SelectToolCommand = new RelayCommand(() => CurrentTool = EditorTool.Select);
            LedgeToolCommand = new RelayCommand(() => CurrentTool = EditorTool.Ledge);

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

        public RelayCommand AddObjectCommand { get; }
        public RelayCommand DeleteObjectCommand { get; }
        public RelayCommand AddRotatorCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }
        public RelayCommand SelectToolCommand { get; }
        public RelayCommand LedgeToolCommand { get; }

        /// <summary>Caminho do arquivo da cena atual, se salva/aberta em disco.</summary>
        public string? CurrentPath { get; private set; }

        public GameObject? SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject == value)
                    return;

                if (_selectedObject != null)
                    _selectedObject.IsSelected = false;

                _selectedObject = value;

                if (_selectedObject != null)
                    _selectedObject.IsSelected = true;

                if (_selectedObject != null && _selectedLedge != null)
                {
                    _selectedLedge = null;
                    OnPropertyChanged(nameof(SelectedLedge));
                }

                Inspector.Target = _selectedObject;
                OnPropertyChanged();
                DeleteObjectCommand.RaiseCanExecuteChanged();
                AddRotatorCommand.RaiseCanExecuteChanged();
            }
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
                }
            }
        }
        public bool IsSelectTool => _currentTool == EditorTool.Select;
        public bool IsLedgeTool => _currentTool == EditorTool.Ledge;

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

        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(ref _isPlaying, value);
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
            var obj = SelectedObject;
            if (obj != null)
            {
                History.Do(new EditorAction("Excluir objeto",
                    doAction: () => { if (SelectedObject == obj) SelectedObject = null; Scene.Remove(obj); },
                    undoAction: () => { Scene.Add(obj); SelectedObject = obj; }));
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

        /// <summary>Duplica o objeto selecionado (com seus componentes) — reversível.</summary>
        public void DuplicateSelected()
        {
            var src = SelectedObject;
            if (src == null)
                return;

            var clone = new GameObject(src.Name + " (cópia)");
            clone.Transform.Position = src.Transform.Position + new Vector2(16, 16);
            clone.Transform.Rotation = src.Transform.Rotation;
            clone.Transform.Scale = src.Transform.Scale;

            foreach (var component in src.Components)
            {
                if (component is SpriteRenderer sprite)
                    clone.AddComponent(new SpriteRenderer { Size = sprite.Size, Color = sprite.Color });
                else if (component is RotatorBehavior rotator)
                    clone.AddComponent(new RotatorBehavior { Speed = rotator.Speed });
            }

            History.Do(new EditorAction("Duplicar objeto",
                doAction: () => { Scene.Add(clone); SelectedObject = clone; },
                undoAction: () => { if (SelectedObject == clone) SelectedObject = null; Scene.Remove(clone); }));
        }

        /// <summary>Move o objeto selecionado por um delta (setas do teclado) — reversível.</summary>
        public void Nudge(Vector2 delta)
        {
            var obj = SelectedObject;
            if (obj == null)
                return;

            var from = obj.Transform.Position;
            var to = from + delta;
            obj.Transform.Position = to;
            PushMove(obj, from, to);
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
