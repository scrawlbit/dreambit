using System;
using System.Collections.ObjectModel;
using DreamBit.Engine.Notification;
using DreamBit.Engine.Rendering;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Elements
{
    /// <summary>
    /// Objeto de cena. Porta o design de DreamBit.Game.Elements.GameObject: bindável,
    /// com hierarquia (Children), componentes e estado de editor (IsSelected/IsExpanded).
    /// </summary>
    public sealed class GameObject : NotificationObject
    {
        private string _name = "GameObject";
        private string _tag = string.Empty;
        private int _sortOrder;
        private int _renderLayer;
        private bool _screenSpace;
        private bool _isVisible = true;
        private bool _isSelected;
        private bool _isExpanded = true;
        private GameObject? _parent;

        private readonly ObservableCollection<GameObject> _children = new();
        private readonly ObservableCollection<SceneComponent> _components = new();

        public GameObject()
        {
            Id = Guid.NewGuid();
            Transform = new Transform();
            Children = new ReadOnlyObservableCollection<GameObject>(_children);
            Components = new ReadOnlyObservableCollection<SceneComponent>(_components);
        }

        public GameObject(string name) : this() => _name = name;

        public Guid Id { get; internal set; }
        public Transform Transform { get; }
        public ReadOnlyObservableCollection<GameObject> Children { get; }
        public ReadOnlyObservableCollection<SceneComponent> Components { get; }

        /// <summary>Cena a que este objeto pertence (definida ao ser adicionado).</summary>
        public Scene? Scene { get; internal set; }

        internal void SetScene(Scene? scene)
        {
            Scene = scene;
            foreach (var child in _children)
                child.SetScene(scene);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        /// <summary>
        /// Rótulo livre para classificar o objeto (ex.: "Player", "Enemy", "Hazard").
        /// Usado por triggers/eventos para filtrar quem dispara, sem depender do tipo de
        /// componente. Vazio = sem tag.
        /// </summary>
        public string Tag
        {
            get => _tag;
            set => Set(ref _tag, value ?? string.Empty);
        }

        /// <summary>True se o objeto tem a tag informada (comparação exata, sem diferenciar caixa).</summary>
        public bool HasTag(string tag) => !string.IsNullOrEmpty(tag)
            && string.Equals(_tag, tag, StringComparison.OrdinalIgnoreCase);

        /// <summary>Ordem de desenho (z-order): menor desenha atrás, maior na frente. Padrão 0.</summary>
        public int SortOrder
        {
            get => _sortOrder;
            set => Set(ref _sortOrder, value);
        }

        /// <summary>Camada de render (grossa): desenhada antes do <see cref="SortOrder"/>. Use
        /// para separar fundo / cenário / jogo / frente (ex.: -100 fundo, 0 padrão, 100 frente).
        /// Equivale às Sorting Layers do Unity / camadas do Godot.</summary>
        public int RenderLayer
        {
            get => _renderLayer;
            set => Set(ref _renderLayer, value);
        }

        /// <summary>
        /// Se true, o objeto é desenhado fixo na tela (HUD), ignorando a câmera: sua posição
        /// vira coordenada de tela (pixels a partir do canto superior-esquerdo). Se false
        /// (padrão), anda com o mundo/câmera. Vale para qualquer componente (sprite, texto,
        /// barra de vida). Herdado pelos filhos: um painel HUD leva seus ícones junto.
        /// </summary>
        public bool ScreenSpace
        {
            get => _screenSpace;
            set { if (Set(ref _screenSpace, value)) OnPropertyChanged(nameof(EffectiveScreenSpace)); }
        }

        /// <summary>True se este objeto, ou algum ancestral, está em espaço de tela (HUD).</summary>
        public bool EffectiveScreenSpace => _screenSpace || (_parent?.EffectiveScreenSpace ?? false);

        public bool IsVisible
        {
            get => _isVisible;
            set => Set(ref _isVisible, value);
        }
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
        public bool IsExpanded
        {
            get => _isExpanded;
            set => Set(ref _isExpanded, value);
        }
        public GameObject? Parent
        {
            get => _parent;
            private set => Set(ref _parent, value);
        }

        public void AddChild(GameObject child)
        {
            if (child == this)
                throw new InvalidOperationException("Um objeto não pode ser filho de si mesmo.");

            child._parent?.RemoveChild(child);
            child.Parent = this;
            child.Transform.Parent = Transform;
            _children.Add(child);
            child.SetScene(Scene);
        }

        public void RemoveChild(GameObject child)
        {
            if (_children.Remove(child))
            {
                child.Parent = null;
                child.Transform.Parent = null;
                child.SetScene(null);
            }
        }

        public T AddComponent<T>(T component) where T : SceneComponent
        {
            component.Owner = this;
            _components.Add(component);
            return component;
        }

        public void RemoveComponent(SceneComponent component)
        {
            _components.Remove(component);
        }

        internal void StartPlay()
        {
            foreach (var component in _components)
                component.OnPlayStarted();

            foreach (var child in _children)
                child.StartPlay();
        }

        internal void Update(GameTime gameTime)
        {
            foreach (var component in _components)
                if (component.Enabled)
                    component.Update(gameTime);

            foreach (var child in _children)
                child.Update(gameTime);
        }

        internal void Draw(ISceneDrawing drawing)
        {
            if (!IsVisible)
                return;

            foreach (var component in _components)
                if (component.Enabled)
                    component.Draw(drawing);

            foreach (var child in _children)
                child.Draw(drawing);
        }

        /// <summary>Desenha só os componentes deste objeto (sem recorrer aos filhos) — usado
        /// pela ordenação global de z-order.</summary>
        internal void DrawSelf(ISceneDrawing drawing)
        {
            foreach (var component in _components)
                if (component.Enabled)
                    component.Draw(drawing);
        }
    }
}
